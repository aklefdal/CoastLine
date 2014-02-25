// http://www.ngdc.noaa.gov/mgg/gdas/gx_announce.Html

namespace CoastLine

open System
open System.IO
open System.Globalization

module Data =

    let ni = CultureInfo.InstalledUICulture.NumberFormat.Clone() :?> NumberFormatInfo
    ni.NumberDecimalSeparator <- "."
    
    let ParseDouble s = Double.Parse(s, ni)

    let private FilterCount minCount groups =
        groups
        |> Array.filter (fun g -> Array.length g > minCount)

    let private ToPoints (lines : array<string>) =
        lines
        |> Array.Parallel.map (fun line -> line.Split [|'\t'|])
        |> Array.Parallel.map (fun items -> items |> Array.map ParseDouble) 
        |> Array.choose (fun doubles -> match doubles with
                                        | [|long; lat|] -> Some {Long=long; Lat=lat}
                                        | _ -> None)

    let private ReadData fileName =
        fileName
        |> File.ReadAllLines
        |> Array.breakOn (fun line -> line = "nan nan")
        |> Array.Parallel.map ToPoints
        |> FilterCount 100

    let Simplify e polyLines =
        polyLines
        |> Array.Parallel.map (fun p -> 
            p |> Reduce.Reduce e)
         
    let GetCoast fileName = 
        let polyLines = ReadData fileName
        polyLines
