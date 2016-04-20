#r "..\packages/Suave/lib/net40/Suave.dll"
#r "..\packages/Suave.Experimental/lib/net40/Suave.Experimental.dll"
    
open Suave                 // always open suave

open Suave.Operators
open Suave.Successful      // for OK-result
open Suave.Filters
open Suave.Web             // for config
open Suave.Form
open Suave.Model.Binding
open Suave.RequestErrors

#load "Form.fs"
#load "View.fs"

open FSharpForms
open FSharpForms

let html container =
    OK (container)
    >=> Writers.setMimeType "text/html; charset=utf-8"

let bindToForm form handler =
    bindReq (bindForm form) handler BAD_REQUEST
    
let experimentalHuman : WebPart =
    choose [
        GET >=>  warbler (fun _ -> OK (Views.createHuman) )
        POST >=> bindToForm Forms.human (fun form -> OK ( Views.showHuman form) ) 
    ]
              
let app = 
        choose [
            path "/" >=> (OK "Homes2")
            path "/experimental" >=> experimentalHuman
        ]
