#!/usr/bin/env -S dotnet fsi

#r "nuget: FSharpMyExt, 2.0.0-prerelease.11"
#r "nuget: Twine.Twee.FSharp, 0.5"
#load "./lib/GamebookGenerator/src/Core/Ast.fs"
#load "./lib/GamebookGenerator/src/Core/Helpers.fs"
#load "./lib/GamebookGenerator/src/Core/Parser/Common.fs"
#load "./lib/GamebookGenerator/src/Core/Parser/Inline.fs"
#load "./lib/GamebookGenerator/src/Core/Parser/Line.fs"
#load "./lib/GamebookGenerator/src/Core/Parser/Paragraph.fs"
#load "./lib/GamebookGenerator/src/Core/Parser/Document.fs"
#load "./lib/GamebookGenerator/src/Twine/SugarCube/Ast.fs"
#load "./lib/GamebookGenerator/src/Twine/SugarCube/Helpers.fs"
#load "./lib/GamebookGenerator/src/Twine/SugarCube/Printer/Link.fs"
#load "./lib/GamebookGenerator/src/Twine/SugarCube/Printer/Inline.fs"
#load "./lib/GamebookGenerator/src/Twine/SugarCube/Printer/Line.fs"
#load "./lib/GamebookGenerator/src/Twine/SugarCube/Printer/PassageBody.fs"
#load "./lib/GamebookGenerator/src/Twine/Inline.fs"
#load "./lib/GamebookGenerator/src/Twine/Line.fs"
#load "./lib/GamebookGenerator/src/Twine/ParagraphBody.fs"
#load "./lib/GamebookGenerator/src/Twine/Paragraph.fs"
#load "./lib/GamebookGenerator/src/Twine/Document.fs"
open GamebookGenerator.Core.Parser
open GamebookGenerator.Twine
open Twine.Twee.FSharp
open Twine.Twee.FSharp.Printer
open Twine.SugarCube.FSharp
open Twine.SugarCube.FSharp.Printer
open Twine.SugarCube.FSharp.Helpers

module PassageBody =
    let ofLines =
        List.map (fun str -> line [text str])

let storyTitle title : Passage<PassageBody> =
    {
        Header = {
            Name = "StoryTitle"
            Tags = None
            Metadata = None
        }
        Body = PassageBody.ofLines [
            title
        ]
    }

let storyData (startPassage: string) : Passage<PassageBody> =
    {
        Header = {
            Name = "StoryData"
            Tags = None
            Metadata = None
        }
        Body = PassageBody.ofLines [
            "{"
            "  \"ifid\": \"5ED897FD-52B4-4903-AEA8-B264106999CC\"," // todo: generate IFID
            "  \"format\": \"SugarCube\","
            "  \"format-version\": \"2.37.3\","
            $"  \"start\": \"{startPassage}\","
            "  \"zoom\": 1"
            "}"
        ]
    }

let gbIntroduction body : Passage<PassageBody> =
    {
        Header = {
            Name = "gb-introduction"
            Tags = None
            Metadata = None
        }
        Body = body
    }

let gbSettings : Passage<PassageBody> =
    let css =
        [
            "p {"
            "  text-align: justify;"
            "  text-align-last: left;"
            "}"
            ""
            ".cover_title {"
            "  font-size: 4.5em;"
            "}"
        ]
        |> String.concat "\\r\\n"
        |> sprintf "\"%s\""

    {
        Header = {
            Name = "gb-settings"
            Tags = None
            Metadata = None
        }
        Body = PassageBody.ofLines [
            "{"
            "    \"end_text\": \"THE END\","
            "    \"death_text\": \"YOU DIED\","
            "    \"separator\": \"0\","
            "    \"break\": \"0\","
            $"    \"css\": %s{css},"
            "    \"page_size\": \"A5\","
            "    \"cover\": \"1\","
            "    \"mdtype\": \"sugarcube\","
            "    \"resolution\": \"300\","
            "    \"image_resolution\": \"300\","
            "    \"low_res\": false,"
            "    \"footers\": \"numbers\","
            "    \"links\": {"
            "        \"Turnto\": \"\","
            "        \"turnto\": \"§{number}\","
            "        \"Returnto\": \"\","
            "        \"returnto\": null,"
            "        \"default\": \"\""
            "    },"
            "    \"margin_top\": \"15\","
            "    \"margin_left\": \"15\","
            "    \"margin_right\": \"15\","
            "    \"margin_bottom\": \"15\","
            "    \"margin_print_left\": \"20\","
            "    \"margin_print_right\": \"10\""
            "}"
        ]
    }

let gbFrontCover bookName bookNameAdded author : Passage<PassageBody> =
    {
        Header = {
            Name = "gb-front-cover"
            Tags = None
            Metadata = None
        }
        Body = PassageBody.ofLines [
            "<div class='cover_top'>"
            $"  <h1 class='cover_title'>%s{bookName}</h1>"
            $"  <h2>%s{bookNameAdded}</h2>"
            "</div>"
            "<div class='cover_image'></div>"
            "<div class='cover_bottom'>"
            $"  <h2>%s{author}</h2>"
            "</div>"
        ]
    }

let bookName = "Александр"
let bookNameAdded = "Как бороться с тиранами,<br>но случайно стать одним из них"
let author = "Пётр Прокошев"

let convert () =
    Document.parseFile @"src/Александр.txt"
    |> Result.map (fun (result, _, _) -> result)
    |> Result.map Document.toTwine
    |> Result.map (fun document ->
        let document =
            match document with
            | x::xs ->
                gbIntroduction x.Body :: xs
            | [] -> []
        [
            gbFrontCover bookName bookNameAdded author
            storyTitle bookName
            storyData "Start"
            gbSettings
            yield! document
        ]
    )
    |> Result.map (
        Document.toString
            PassageBody.shows
            NewlineType.Lf
    )
    |> Result.iter (fun content ->
        let path = "Александр.twee"
        System.IO.File.WriteAllText(path, content)
    )

let duplicates () =
    Document.parseFile @"src/Александр.txt"
    |> Result.map (fun (result, _, _) ->
        result.Paragraphs
        |> List.groupBy (fun x -> x.Id)
        |> List.choose (fun (id, xs) ->
            if List.length xs <= 1 then
                None
            else
                Some [string id; string <| List.length xs]
        )
        |> fun xs -> ["Параграф"; "Дублей"]::xs
        |> FsharpMyExtension.Primitives.String.createTable 2
        |> printfn "%s"
    )

convert ()
