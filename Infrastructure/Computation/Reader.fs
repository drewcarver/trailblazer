namespace HikePlanner.Infrastructure.Computation

open System

type Reader<'env, 'a> = Reader of ('env -> 'a)

module Reader =
    let run (env: 'env) (Reader f) = f env

    let bind (f: 'a -> Reader<'env, 'b>) (reader: Reader<'env, 'a>) : Reader<'env, 'b> =
        Reader (fun env ->
            let a = run env reader
            run env (f a))

    let result (x: 'a) : Reader<'env, 'a> =
        Reader (fun _ -> x)

    let map (f: 'a -> 'b) (reader: Reader<'env, 'a>) : Reader<'env, 'b> =
        Reader (fun env -> f (run env reader))

    let apply (f: Reader<'env, 'a -> 'b>) (reader: Reader<'env, 'a>) : Reader<'env, 'b> =
        Reader (fun env -> (run env f) (run env reader))

    let ask : Reader<'env, 'env> = Reader id

    let asks (f: 'env -> 'a) : Reader<'env, 'a> =
        Reader (fun env -> f env)

    let local (f: 'env -> 'env) (reader: Reader<'env, 'a>) : Reader<'env, 'a> =
        Reader (fun env -> run (f env) reader)

    let map2 (f: 'a -> 'b -> 'c) (r1: Reader<'env, 'a>) (r2: Reader<'env, 'b>) : Reader<'env, 'c> =
        Reader (fun env -> f (run env r1) (run env r2))

    let sequence (readers: Reader<'env, 'a> list) : Reader<'env, 'a list> =
        Reader (fun env -> List.map (fun r -> run env r) readers)

type ReaderBuilder() =
    member _.Bind(reader, f) = Reader.bind f reader
    member _.Return(x) = Reader.result x
    member _.ReturnFrom(x) = x
    member _.Zero() = Reader.result ()
    member _.Combine(r1, r2) = Reader.bind (fun () -> r2) r1
    member _.Delay(f) = f
    member _.Run(f: unit -> Reader<'env, 'a>) = f()
    member _.Using(resource: #IDisposable, f: #IDisposable -> Reader<'env, 'a>) =
        Reader (fun env ->
            try
                let r = f resource
                Reader.run env r
            finally
                match resource with | null -> () | d -> (d :> IDisposable).Dispose())

[<AutoOpen>]
module ReaderExtensions =
    let reader = ReaderBuilder()
