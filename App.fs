namespace HikePlanner.App

open System
open System.Threading.Tasks

type App<'env, 'err, 'a> =
    App of ('env -> Task<Result<'a, 'err>>)

[<RequireQualifiedAccess>]
module App =

    //--------------------------------------------------------------------------
    // Running
    //--------------------------------------------------------------------------

    let run env (App f) =
        f env

    //--------------------------------------------------------------------------
    // Constructors
    //--------------------------------------------------------------------------

    let inline succeed x =
        App(fun _ ->
            task {
                return Ok x
            })

    let inline fail err =
        App(fun _ ->
            task {
                return Error err
            })

    let ofResult (result: Result<'a, 'err>) =
        App(fun _ ->
            task {
                return result
            })

    let ofTask (t: Task<'a>) =
        App(fun _ ->
            task {
                let! x = t
                return Ok x
            })

    let ofAsync (a: Async<'a>) =
        App(fun _ ->
            task {
                let! x = a
                return Ok x
            })

    let ofReader (f: 'env -> 'a) =
        App(fun env ->
            task {
                return Ok (f env)
            })

    //--------------------------------------------------------------------------
    // Environment
    //--------------------------------------------------------------------------

    let ask<'env, 'err> : App<'env, 'err, 'env> =
        App(fun env ->
            task {
                return Ok env
            })

    let asks f =
        App(fun env ->
            task {
                return Ok (f env)
            })

    let local transform (App m) =
        App(fun env ->
            m (transform env))

    //--------------------------------------------------------------------------
    // Functor
    //--------------------------------------------------------------------------

    let map f (App m) =
        App(fun env ->
            task {
                let! result = m env

                return
                    match result with
                    | Ok x -> Ok(f x)
                    | Error e -> Error e
            })

    let mapError f (App m) =
        App(fun env ->
            task {
                let! result = m env

                return
                    match result with
                    | Ok x -> Ok x
                    | Error e -> Error(f e)
            })

    //--------------------------------------------------------------------------
    // Monad
    //--------------------------------------------------------------------------

    let bind f (App m) =
        App(fun env ->
            task {
                let! result = m env

                match result with
                | Error e ->
                    return Error e

                | Ok value ->
                    let (App next) = f value
                    return! next env
            })

    //--------------------------------------------------------------------------
    // Helpers
    //--------------------------------------------------------------------------

    let tap f =
        bind (fun x ->
            f x
            succeed x)

    let tapTask f =
        bind (fun x ->
            App(fun _ ->
                task {
                    do! f x
                    return Ok x
                }))

    let require predicate error =
        bind (fun x ->
            if predicate x then
                succeed x
            else
                fail error)

    let catch (handler: exn -> 'err) (operation: unit -> Task<'a>) =
        App(fun _ ->
            task {
                try
                    let! value = operation ()
                    return Ok value
                with ex ->
                    return Error(handler ex)
            })

type AppBuilder() =

    member _.Return(x) =
        App.succeed x

    member _.ReturnFrom(x: App<'env, 'err, 'a>) =
        x

    member _.Bind
        (
            app: App<'env, 'err, 'a>,
            binder: 'a -> App<'env, 'err, 'b>
        ) =
        App.bind binder app

    member _.Zero() =
        App.succeed ()

    member _.Delay(f) =
        App.bind f (App.succeed ())

    member _.Combine(a, b) =
        App.bind (fun () -> b) a

    member _.While(guard, body) =
        if not (guard()) then
            App.succeed ()
        else
            App.bind (fun () -> _.While(guard, body)) body

    member _.For(sequence: seq<'a>, body) =
        let enumerator = sequence.GetEnumerator()

        _.While(
            (fun () -> enumerator.MoveNext()),
            _.Delay(fun () -> body enumerator.Current)
        )

    member _.TryWith(body, handler) =
        try
            body ()
        with ex ->
            handler ex

    member _.TryFinally(body, compensation) =
        try
            body ()
        finally
            compensation ()

    member _.Using(resource: #IDisposable, body) =
        _.TryFinally(
            (fun () -> body resource),
            (fun () ->
                if not (isNull (box resource)) then
                    resource.Dispose())
        )

    //----------------------------------------------------------------------
    // Automatic lifting
    //----------------------------------------------------------------------

    member _.Source(app: App<'env, 'err, 'a>) =
        app

    member _.Source(result: Result<'a, 'err>) =
        App.ofResult result

    member _.Source(task: Task<'a>) =
        App.ofTask task

    member _.Source(async: Async<'a>) =
        App.ofAsync async

let app = AppBuilder()
