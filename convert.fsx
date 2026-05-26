#!/usr/bin/env -S dotnet fsi

#r "nuget: FSharpMyExt, 2.0.0-prerelease.11"
#r "nuget: Twee.FSharp, 0.4"
#r @"Z:\projects\GamebookGenerator\src\Core\bin\Debug\net6.0\GamebookGenerator.Core.dll"
#r @"Z:\projects\GamebookGenerator\src\Twine\bin\Debug\net6.0\GamebookGenerator.Twine.dll"
open GamebookGenerator.Core.Parser
open GamebookGenerator.Twine

let storyTitle title : Twee.FSharp.Passage =
    {
        Header = {
            Name = "StoryTitle"
            Tags = None
            Metadata = None
        }
        Body = [title]
    }

let storyData (startPassage: string) : Twee.FSharp.Passage =
    {
        Header = {
            Name = "StoryData"
            Tags = None
            Metadata = None
        }
        Body = [
            "{"
            "  \"ifid\": \"5ED897FD-52B4-4903-AEA8-B264106999CC\"," // todo: generate IFID
            "  \"format\": \"SugarCube\","
            "  \"format-version\": \"2.37.3\","
            $"  \"start\": \"{startPassage}\","
            "  \"zoom\": 1"
            "}"
        ]
    }

let gbSettings : Twee.FSharp.Passage =
    {
        Header = {
            Name = "gb-settings"
            Tags = None
            Metadata = None
        }
        Body = [
            "{"
            "    \"end_text\": \"THE END\","
            "    \"death_text\": \"YOU DIED\","
            "    \"separator\": \"0\","
            "    \"break\": \"0\","
            "    \"css\": \"\","
            "    \"page_size\": \"A4-P\","
            "    \"cover\": \"0\","
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
            "    \"margin_left\": \"10\","
            "    \"margin_right\": \"10\","
            "    \"margin_bottom\": \"15\","
            "    \"margin_print_left\": \"20\","
            "    \"margin_print_right\": \"10\""
            "}"
        ]
    }

let convert () =
    Document.parseFile @"src/Александр.txt"
    |> Result.map (fun (result, _, _) -> result)
    |> Result.map Document.toTwine
    |> Result.map (fun document ->
        [
            storyTitle "АЛЕКСАНДР И АНТИЧНЫЙ БАРДАК"
            storyData "Start"
            gbSettings
            yield! document
        ]
    )
    |> Result.map (
        Twee.FSharp.Document.toString Twee.FSharp.NewlineType.Lf
    )
    |> Result.iter (fun content ->
        let path = "Александр.twee"
        System.IO.File.WriteAllText(path, content)
    )

convert ()
