namespace HikePlanner

open System
open System.Threading.Tasks

type TaskReader<'env, 'a> = TaskReader of ('env -> Task<'a>)

module TaskReader =
    let run (env: 'env) (TaskReader f) = f env

    let bind (f: 'a -> TaskReader<'env, 'b>) (tr: TaskReader<'env, 'a>) : TaskReader<'env, 'b> =
        TaskReader (fun env ->
            task {
                let! a = run env tr
                return! run env (f a)
            })

    let result (x: 'a) : TaskReader<'env, 'a> =
        TaskReader (fun _ -> Task.FromResult x)

    let map (f: 'a -> 'b) (tr: TaskReader<'env, 'a>) : TaskReader<'env, 'b> =
        TaskReader (fun env -> task {
            let! a = run env tr
            return f a
        })

    let apply (f: TaskReader<'env, 'a -> 'b>) (tr: TaskReader<'env, 'a>) : TaskReader<'env, 'b> =
        TaskReader (fun env -> task {
            let! f' = run env f
            let! a = run env tr
            return f' a
        })

    let ask : TaskReader<'env, 'env> = TaskReader (fun env -> Task.FromResult env)

    let asks (f: 'env -> 'a) : TaskReader<'env, 'a> =
        TaskReader (fun env -> Task.FromResult (f env))

    let local (f: 'env -> 'env) (tr: TaskReader<'env, 'a>) : TaskReader<'env, 'a> =
        TaskReader (fun env -> run (f env) tr)

    let liftTask (t: Task<'a>) : TaskReader<'env, 'a> =
        TaskReader (fun _ -> t)

    let liftReader (r: Reader<'env, 'a>) : TaskReader<'env, 'a> =
        TaskReader (fun env -> Task.FromResult (Reader.run env r))

type TaskReaderBuilder() =
    member _.Bind(tr: TaskReader<'env, 'a>, f: 'a -> TaskReader<'env, 'b>) =
        TaskReader.bind f tr

    member _.Bind(t: Task<'a>, f: 'a -> TaskReader<'env, 'b>) =
        TaskReader.bind f (TaskReader.liftTask t)

    member _.Return(x) = TaskReader.result x
    member _.ReturnFrom(tr: TaskReader<'env, 'a>) = tr
    member _.ReturnFrom(t: Task<'a>) = TaskReader.liftTask t
    member _.Zero() = TaskReader.result ()
    member _.Combine(r1: TaskReader<'env, unit>, r2: TaskReader<'env, 'a>) =
        TaskReader.bind (fun () -> r2) r1
    member _.Delay(f) = f
    member _.Run(f: unit -> TaskReader<'env, 'a>) = f()
    member _.Using(resource: #IDisposable, f: #IDisposable -> TaskReader<'env, 'a>) =
        TaskReader (fun env ->
            task {
                try
                    let r = f resource
                    return! TaskReader.run env r
                finally
                    match resource with | null -> () | d -> d.Dispose()
            })

[<AutoOpen>]
module TaskReaderExtensions =
    let taskReader = TaskReaderBuilder()
