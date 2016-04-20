module FSharpForms.Views

open System

open Suave.Html
open Suave.Form
open FSharpForms.Forms

let createHuman = 
    html [
        head [
            title "Forms with Experimental"
        ]

        body [           
            div [text "Create"]            
            
            tag "form" ["method", "POST"] (flatten 
                [
                    tag "fieldset" [] (flatten 
                        [
                            divAttr ["class", "editor-label"] [
                                text "Name"
                            ]
                            divAttr ["class", "editor-field"] [
                                input (fun f -> <@ f.Name @>) [] Forms.human
                            ]
                            
                            divAttr ["class", "editor-label"] [
                                text "Surname"
                            ]
                            divAttr ["class", "editor-field"] [
                                input (fun f -> <@ f.Surname @>) [] Forms.human
                            ]
                        ])
                        
                    inputAttr ["type", "submit"; "value", "Create human"]                
                ])
       ]
    ]
    |> xmlToString
    
let showHuman human = 
    html [
        head [
            title "Forms with Experimental"
        ]

        body [           
            div [text "Show"]
            div [text (sprintf "Name: %s" human.Name)]
            div [text (sprintf "Name: %s" human.Surname)]
       ]
    ]
    |> xmlToString
