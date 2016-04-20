module FSharpForms.Forms

open Suave.Form


type Human = {
    Name : string
    Surname : string
}

let human : Form<Human> = 
    Form ([ TextProp ((fun f -> <@ f.Name @>), [])
            TextProp ((fun f -> <@ f.Surname @>), [ ])
            ],
          [])