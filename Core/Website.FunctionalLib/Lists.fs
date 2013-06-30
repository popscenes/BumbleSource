namespace Website.FunctionalLib

type Lists() = 
    let rec cartesian = function
    | [] -> [[]]
    | L::Ls -> [for C in cartesian Ls do yield! [for x in L do yield x::C]]

    member this.Cartesian l = cartesian l
