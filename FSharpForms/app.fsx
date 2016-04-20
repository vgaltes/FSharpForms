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
   
let experimentalHuman =
    choose [
        GET >=>  OK (Views.createHuman)
        POST >=> bindReq (bindForm Forms.human) (fun form -> OK ( Views.showHuman form) ) BAD_REQUEST
    ]
              
let app = 
        choose [
            path "/" >=> (OK "Homes2")
            path "/experimental" >=> experimentalHuman
        ]
