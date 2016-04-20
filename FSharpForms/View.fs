module FSharpForms.Views

open System

open Suave.Html
open Suave.Form
open FSharpForms.Forms

let divClass c = divAttr ["class", c]
let form x = tag "form" ["method", "POST"] (flatten x)
let submitInput value = inputAttr ["type", "submit"; "value", value]
let fieldset x = tag "fieldset" [] (flatten x)
let legend txt = tag "legend" [] (text txt)

type Field<'a> = {
    Label : string
    Xml : Form<'a> -> Suave.Html.Xml
}

type Fieldset<'a> = {
    Legend : string
    Fields : Field<'a> list
}

type FormLayout<'a> = {
    Fieldsets : Fieldset<'a> list
    SubmitText : string
    Form : Form<'a>
}

let renderForm (layout : FormLayout<_>) =    
    form [
        for set in layout.Fieldsets -> 
            fieldset [
                yield legend set.Legend

                for field in set.Fields do
                    yield divClass "editor-label" [
                        text field.Label
                    ]
                    yield divClass "editor-field" [
                        field.Xml layout.Form
                    ]
            ]

        yield submitInput layout.SubmitText
    ]
    
let createHuman = 
    html [
        head [
            title "Forms with Experimental"
        ]

        body [           
            div [text "Create"]

            renderForm
                { Form = Forms.human
                  Fieldsets = 
                    [ { Legend = "Album"
                        Fields = 
                            [ { Label = "Name"
                                Xml = input (fun f -> <@ f.Name @>) [] }
                              { Label = "Surname"
                                Xml = input (fun f -> <@ f.Surname @>) [] }
                            ] } ]
                  SubmitText = "Create" }
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
