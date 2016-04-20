// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"
#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
#r "packages/Suave/lib/net40/Suave.dll"

open System
open System.IO
open Suave
open Suave.Web
open Microsoft.FSharp.Compiler.Interactive.Shell
open Fake


// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
      ++ "/**/*.fsproj"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
)

// --------------------------------------------------------------------------------------
// The following uses FileSystemWatcher to look for changes in 'app.fsx'. When
// the file changes, we run `#load "app.fsx"` using the F# Interactive service
// and then get the `App.app` value (top-level value defined using `let app = ...`).
// The loaded WebPart is then hosted at localhost:8083.
// --------------------------------------------------------------------------------------

let sbOut = new Text.StringBuilder()
let sbErr = new Text.StringBuilder()

let fsiSession =
  let inStream = new StringReader("")
  let outStream = new StringWriter(sbOut)
  let errStream = new StringWriter(sbErr)
  let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
  let argv = Array.append [|"/fake/fsi.exe"; "--quiet"; "--noninteractive"; "-d:DO_NOT_START_SERVER"|] [||]
  FsiEvaluationSession.Create(fsiConfig, argv, inStream, outStream, errStream)

let reportFsiError (e:exn) =
  traceError "Reloading app.fsx script failed."
  traceError (sprintf "Message: %s\nError: %s" e.Message (sbErr.ToString().Trim()))
  sbErr.Clear() |> ignore

let reloadScript () =
  try
    //traceImportant "Minifying JS script"
    //Run "minify"
    
    //Reload application
    traceImportant "Reloading app.fsx script..."
    let appFsx = __SOURCE_DIRECTORY__ @@ "/FSharpForms/app.fsx"
    fsiSession.EvalInteraction(sprintf "#load @\"%s\"" appFsx)
    fsiSession.EvalInteraction("open App")
    match fsiSession.EvalExpression("app") with
    | Some app -> Some(app.ReflectionValue :?> WebPart)
    | None -> failwith "Couldn't get 'app' value"
  with e -> reportFsiError e; None

// --------------------------------------------------------------------------------------
// Suave server that redirects all request to currently loaded version
// --------------------------------------------------------------------------------------

let currentApp = ref (fun _ -> async { return None })

let rec findPort port =
  try
    let tcpListener = System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse("127.0.0.1"), port)
    tcpListener.Start()
    tcpListener.Stop()
    port
  with :? System.Net.Sockets.SocketException as ex ->
    findPort (port + 1)

let getLocalServerConfig port =
  { defaultConfig with
      homeFolder = Some __SOURCE_DIRECTORY__
      logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Debug
      bindings = [ HttpBinding.mkSimple HTTP  "127.0.0.1" port ] }

let reloadAppServer (changedFiles: string seq) =
  traceImportant <| sprintf "Changes in %s" (String.Join(",",changedFiles))
  reloadScript() |> Option.iter (fun app ->
    currentApp.Value <- app
    traceImportant "Refreshed app." )
    
Target "Run" (fun _ ->
  let app ctx = currentApp.Value ctx
  let port = findPort 8083
  let _, server = startWebServerAsync (getLocalServerConfig port) app

  // Start Suave to host it on localhost
  reloadAppServer ["./FSharpForms/app.fsx"]
  Async.Start(server)
  // Open web browser with the loaded file
  System.Diagnostics.Process.Start(sprintf "http://localhost:%d" port) |> ignore
  
  // Watch for changes & reload when app.fsx changes
  let sources = 
    { BaseDirectory = __SOURCE_DIRECTORY__
      Includes = [ "**/*.fsx"; "**/*.fs" ; "**/*.fsproj"; "web/content/app/*.js" ]; 
      Excludes = [] }
      
  use watcher = sources |> WatchChanges (Seq.map (fun x -> x.FullPath) >> reloadAppServer)
  
  traceImportant "Waiting for app.fsx edits. Press any key to stop."

  System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)
)

// Build order
"Clean"
  ==> "Build"
  ==> "Run"
  ==> "Deploy"

// start build
RunTargetOrDefault "Run"
