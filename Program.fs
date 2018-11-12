type Rank =
    | Ace
    | Two
    | Three
    | Four
    | Five
    | Six
    | Seven
    | Eight
    | Nine
    | Ten
    | Jack
    | Queen
    | King

type Suit =
    | Hearts
    | Spades
    | Clubs
    | Diamonds

type Card = Rank * Suit

type CardValue =
    | SingleValue of int
    | DoubleValue of int * int

let suits = [ Hearts ; Spades ; Clubs ; Diamonds ]
let ranks = [ Ace; Two; Three; Four; Five; Six; Seven; Eight; Nine; Ten; Jack; Queen; King ]

type Deck = Card list

let cleanDeck = [for suit in suits do
                         for rank in ranks do
                            yield Card(rank,suit) ]

let getShuffledDeck() =
    let rnd = new System.Random()
    cleanDeck
    |> List.zip <| List.init cleanDeck.Length (fun _ -> rnd.Next())
    |> List.sortBy snd
    |> List.map fst

type Decision =
    | Stand
    | Hit

type PlayerState =
    | NotPlaying
    | Playing of Card list
    | Stood of int
    | Bust of Card list

type Player =
    {
        Name: string
        State: PlayerState
    }

type RoundResult =
    | Won of Player
    | Lost of Player
    | Push of Player

let deal deck =
    match deck with
    | card1 :: card2 :: rest -> card1,card2,rest
    | _ -> failwith "Deck needs at least 2 cards"

let draw deck =
    match deck with
    | card :: rest -> card,rest
    | _ -> failwith "Deck needs at least 1 card"

let dealInitial players deck =
    let dealPlayer deck player =
        let card1,card2,newDeck = deal deck
        let cards = [card1;card2]
        { player with State = Playing(cards) }, newDeck
    List.mapFold dealPlayer deck players

let getRank (card:Card) =
    fst card
let getValue card =
    match getRank card with
    | Ace -> DoubleValue(1,11)
    | Two -> SingleValue(2)
    | Three -> SingleValue(3)
    | Four -> SingleValue(4)
    | Five -> SingleValue(5)
    | Six -> SingleValue(6)
    | Seven -> SingleValue(7)
    | Eight -> SingleValue(8)
    | Nine -> SingleValue(9)
    | Ten | Jack | Queen | King -> SingleValue(10)

type Score =
    | IndeterminateScore of int list
    | NormalScore of int
    | BustScore of int

let calculateScore (cards:Card list) =
    let folder scores score =
        match score with
        | SingleValue(x) -> List.map (fun y -> y + x) scores
        | DoubleValue(x,y) ->
            let left = List.map (fun a -> a + x) scores
            let right = List.map (fun a -> a + y) scores
            left @ right
    let scores = 
        cards
        |> List.map getValue
        |> List.fold folder [0]
        |> List.distinct
    if not (List.exists (fun x -> x < 22) scores) then
        List.min scores |> BustScore
    else if scores.Length > 1 then
        IndeterminateScore scores
    else
        NormalScore scores.Head
let isNotBust score =
    match score with 
    | BustScore _ -> false
    | NormalScore _ -> true
    | IndeterminateScore _ -> true

let getBestNonBustScore score =
    match score with 
    | BustScore _ -> failwith "This function doesn't handle bust scores"
    | NormalScore s -> s
    | IndeterminateScore scores -> 
        scores
        |> List.filter (fun x -> x < 22)
        |> List.max

let rec getPlayerDecision cards visibleDealerCard =
    printfn ""
    printfn "Dealer has %A and your cards are %A (S)tand or (H)it?" visibleDealerCard cards
    let response = System.Console.ReadKey()
    printf "\b"
    match response.KeyChar with
        | 'S' | 's' -> Stand
        | 'H' | 'h' -> Hit
        | _ ->
            printfn "Sorry I don't understand"
            getPlayerDecision cards visibleDealerCard

let rec processPlayer visibleDealerCard makeDecision deck player =
    match player.State with
    | Playing(cards) ->
        match makeDecision cards visibleDealerCard with
            | Hit ->
                printfn "%s Hits" player.Name
                let card,remaining = draw deck
                let newPlayerCards = card::cards
                let scores = calculateScore newPlayerCards
                if isNotBust scores then
                    processPlayer  visibleDealerCard makeDecision remaining {player with State = Playing(newPlayerCards) }
                else
                    printfn "Bust! %A" newPlayerCards
                    { player with State = Bust(newPlayerCards) }, remaining
            | Stand ->
                let finalScore = cards |> calculateScore |> getBestNonBustScore
                printfn "%s stands at %i" player.Name finalScore
                { player with State = Stood(finalScore) }, deck
    | _ -> failwith "Player is not currently playing"

let isOn17orBetter cards =
    let score = calculateScore cards
    match score with
    | NormalScore s -> s > 16
    | IndeterminateScore scores -> List.exists (fun x -> x > 16 && x < 22) scores
    | BustScore _ -> failwith "Shouldn't call this function with a bust score"

let rec getDealerDecision cards _ =
     printfn "Dealer Cards: %A" cards
     if isOn17orBetter cards then
        Stand
     else
        Hit

let getRoundResult dealer player =
    match dealer.State, player.State with
    | Stood(dealerScore), Stood(playerScore) -> 
        if playerScore > dealerScore then
            Won(player)
        else if playerScore = dealerScore then
            Push(player)
        else
            Lost(player)
    | Bust(_), Stood(_) -> Won(player)
    | _,_ -> Lost(player)

let printRoundResult result =
    match result with
    | Won(p) -> printfn "%s won!" p.Name
    | Lost(p) -> printfn "%s lost!" p.Name
    | Push(p) -> printfn "%s drew with the dealer!" p.Name

let playGame() =
    let deck = getShuffledDeck()
    let players, newDeck = dealInitial [ { Name = "Richard"; State = NotPlaying } ] deck
    let visibleDealerCard, hiddenDealerCard, remainingDeck = deal newDeck
    let dealer = { Name = "Dealer"; State = Playing([visibleDealerCard;hiddenDealerCard]) }
    let playerResults, deck = List.mapFold (processPlayer visibleDealerCard getPlayerDecision) remainingDeck players
    let dealerResult, deck = processPlayer visibleDealerCard getDealerDecision deck dealer
    let roundResults = playerResults |> List.map (getRoundResult dealerResult)
    List.iter printRoundResult roundResults

[<EntryPoint>]
let main argv =
    playGame()
    System.Console.Read() |> ignore
    0 // return an integer exit code