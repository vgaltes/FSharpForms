#r "..\packages/Suave/lib/net40/Suave.dll"
#r "..\packages/Suave.Experimental/lib/net40/Suave.Experimental.dll"
#r "..\packages/DotLiquid/lib/NET45/DotLiquid.dll"
#r "..\packages/Suave.DotLiquid/lib/net40/Suave.DotLiquid.dll"    
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

DotLiquid.setTemplatesDir (__SOURCE_DIRECTORY__ + "./Templates")
   
let experimentalHuman =
    choose [
        GET >=>  OK (Views.createHuman)
        POST >=> bindReq (bindForm Forms.human) (fun form -> OK ( Views.showHuman form) ) BAD_REQUEST
    ]
    
let dotliquidHuman =
     choose [
        GET >=>  DotLiquid.page ("CreateHuman.html") ()
        POST >=> bindReq (bindForm Forms.human) (fun form -> DotLiquid.page ("ShowHuman.html") form ) BAD_REQUEST
    ]
             
let app = 
        choose [
            path "/" >=> (OK "Homes2")
            path "/experimental" >=> experimentalHuman
            path "/dotliquid" >=> dotliquidHuman
        ]
