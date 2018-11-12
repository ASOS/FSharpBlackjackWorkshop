# Welcome to the Blackjack workshop

Congratulations on your new role as programmer for my soon to be online casino.  The first game I'm implementing is blackjack and I need to your help to get it ready!

>Note: This exercise assumes some familiarity with C# and .NET Core

>Note: If you're familiar with F# you may notice that there are several places where this design could be improved.  This is partly to make the code more understandable for a new comer and partly so it can be improved in a later version of the workshop.

## A crash course in F#
If you don't want to read this then feel free to skip straight to the exercises as the code to complete them is displayed inline so you will be able to complete the tasks even if you don't understand the code fully but there are a few points that might be helpful if you get stuck.

### Union types / Algebraic Types / Discriminated Unions

Simply put these are types that can be a number of things.

```fsharp
type Suit =
    | Hearts
    | Spades
    | Clubs
    | Diamonds
```
This is a bit like an enumeration in C# in that an instance of the suit type has to be one of those values.  These values can be empty labels like they are here or they can of a certain type.

```fsharp
type PlayerState =
    | NotPlaying
    | Playing of Card list
    | Stood of int
    | Bust of Card list
```

In this instance if you want to create a player state of playing then you must supply a card list as you can't be playing blackjack if you don't have any cards.

### Tuples

We have them in C# as well but it's worth mentioning they have special syntax in F#

```CSharp
var tuple = new Tuple<int,string>(6,"a");
```
is equivalent to
```FSharp
let tuple = 6,"a"
```

### Lists

F#'s default collection type is a linked list which is represented using items in square brackets separated by semi colons e.g.

```FSharp
let list = [1;2;3]
let result = 0 :: list // Evaluates to [0; 1; 2; 3]
let identicalList = 1 :: 2 :: 3 :: []
let areIdentical = list = identicalList // Evaluates to true
```
>Note: The equals sign in the last line of the above example performs a comparison and not an assignment i.e. it is equivalent to == in C#.  This is why the operation evaluates to a Boolean.

The `::` is slightly odd in that all it does is append a value to the start of a linked list.  It's not much use on its own but it's main use is in pattern matching as we'll see below.

### Matching

The match statement which you will see littered throughout F# code is a sort of super powered switch statement.  First let's look at a simple example.

```FSharp
let input = 5
match input with
| 1 -> "Output is 1"
| 5 -> "Output is 5"
| a -> "Output is " + a.ToString();
```

Hopefully this is fairly self explanatory.  The outcome to match comes after the `|` and the value to use if the input matches comes after the `->` symbol.

Now let's look at the more complicated example of the draw function that takes a list of cards (the deck parameter) removes 1 card and returns the card that was removed and the rest of the cards.

```FSharp
let draw deck =
    match deck with
    | card :: rest -> card,rest
    | _ -> failwith "Deck needs at least 1 card"
```
In case you hadn't realised draw is the name of the method and deck is the name the parameter passed into that method. So what does this method do?  As we learned above the `::` operator prepends items on to the start of a list.  Let's look at the 3rd line in detail.

The symbol `|` indicates that this a pattern we want to try and match.
`card :: rest` says that we want to bind the first item in the list to the variable `card` and we want to bind the rest of the items in the list to the variable `rest`.  To put it simply this pattern matches any list that isn't empty and then returns a tuple of the card and the rest of the list. 

The second pattern uses and `_` character which in F# means to match anything and ignore it.  It then throws an exception using the `failwith` method which is quick and convenient way to throw an exception.

### Crash course composition

You'll see some strange looking symbols that you've never seen in C# but what they do isn't too complicated.

```FSharp
x |> someFunctionF |> someFunctionG
```
is just the same as
```CSharp
someFunctionG(someFunctionF(x));
```

# Exercises

## Feature 1: Improve the user experience

### Task 1: Print cards out in a friendly manner

Currently the program just outputs the raw data structures for the card. Let's replace that with something more friendly that prints a shortened and more readable version.

#### Step 1: Create a method that takes a card tank and returns a string representation, we'll call it printRank.

Add the following somewhere below the rank type.

```fsharp
let printRank rank =
    match rank with
    | Ace -> "A"
    | Two ->  "2"
    | Three -> "3"
    | Four -> "4"
    | Five -> "5"
    | Six -> "6"
    | Seven -> "7"
    | Eight -> "8"
    | Nine -> "9"
    | Ten -> "10"
    | Jack -> "J"
    | Queen -> "Q"
    | King ->  "K"
```


#### Step 2: Create a method that takes a card suit and returns a string representation, we'll call it printSuit.

This is the same as the task above, but this time we are matching on the Suit type rather than the Rank type.  Have a go at it yourself and look at the solution if you get stuck.

<details>
<summary>Solution</summary>

Add the following somewhere below the suit type.

```fsharp
let printSuit suit =
    match suit with
    | Hearts -> "H"
    | Spades -> "S"
    | Clubs -> "C"
    | Diamonds -> "D"
```

</details>

#### Step 3: Create a method that takes a card and uses printSuit and printRank to create a string representation of the card.

Add the following below the card type.

```fsharp
let printCard (card:Card) =
    let suit = card |> snd |> printSuit
    let rank = card |> fst |> printRank
    sprintf "%s%s" rank suit
```

A couple of things to note here, the first is that snd and fst function return the second and first value of a tuple respectively. The second is that the sprintf function is similar to .NET's String.Format but with one important difference.  The arguments to sprintf are typed so if you put `%s` in the string then sprintf will give you a compiler error if the argument you pass in isn't a string.

#### Step 4: Create a method to print a list of cards

Add the following below the card type.

```fsharp
let printCards (cards:Card list) =
    List.map printCard cards |> List.reduce (fun a b -> a + "," + b)
```

There's a lot going on here so let's try and break it down a bit. List.map is a function that's the F# equivalent of Enumerable.Select in C#.  It takes each item in the list supplied by the 2nd parameter and runs the function supplied in the first parameter to create a new item.  It does this for every item in the list and creates a new list containing the results which it then passes to List.Reduce using the `|>` operator discussed above.

List.reduce is the F# equivalent of Enumerable.Aggregate in C# it takes items and joins them together for form a single item.  In this case we concatenate the list of strings produced by the List.map call into a single string where the items are separated by commas.

#### Step 5: Change the method getPlayerDecision to use the new printCard function 

Replace the top 2 lines of the getPlayerDecision method with the following:

```fsharp
printfn ""
let printedCards = printCards cards
let printedDealerCard =  printCard visibleDealerCard
printfn "Dealer has %s and your cards are %s (S)tand or (H)it?" printedDealerCard printedCards
```

#### Step 6: Change the method getDealerDecision to use the print cards method as well.

Replace the top line of the getDealerDecision method with the following:

```fsharp
let dealerCards = cards |> printCards
printfn "Dealer Cards: %s" dealerCards
```

#### Step 7: Run the solution or use FSharp Interactive to test it

If you're using FSharp Interactive (FSI) then send the entire file except the main method to the interactive window and then type `playGame();;` to run the playGame method.
If you get an error when trying to use FSI then trying running `dotnet restore` and then trying again.
If you're running .NET core this is as simple as typing `dotnet run` in PowerShell in the directory containing the project.

### Task 2: 

Don't make the user add up their own score! Currently they keep having to work it out themselves!

#### Step 1: Create a method call printScore that takes the output of the calculate score method (a value of the Score type) and turns it into something more readable

Add the following below the score type.

```fsharp
let printScore score =
    match score with
    | NormalScore s -> string s
    | BustScore _ -> "Bust"
    | IndeterminateScore l -> l |> List.map string |> List.reduce (fun a b -> a + "/" + b)
```

#### Step 2: Modify the getPlayerDecision to include the calculated score

To print the calculated score we're going to modify the getPlayerDecision method to calculate the score and then print that score.  To do this we'll pass the cards variable to calculate score and then pass the output of the calculate score to the printScore method to get the score as a string.  Let's start some code that gets the printed score as a string using the procedure described above.

Have a go at this without looking at the solution to see if you can do it.  If you get stuck then look at the solution below.

<details>
<summary>Solution</summary>

Add the line below to the getPlayerDecision method.

```fsharp
let printedScore = cards |> calculateScore |> printScore
```

</details>

Now that we have the printed score as a string we just need to add the parameter to printfn statement and change the format string.  Have a go at this yourself and look at the solution if you get stuck.

<details>
<summary>Solution</summary>

Modify the line that prints to be 

```fsharp
printfn "Dealer has %s and your score is %s and your cards are %s (S)tand or (H)it?" printedDealerCard printedScore printedCards
```

</details>

#### Step 3: Modify the getDealerDecision to include the calculated score

Now we're going to modify the getDealerDecision method to print out the dealer's score. This should be more or less the same as the code used before so have a go yourself and look at the solution below if you get stuck.

<details>
<summary>Solution</summary>

Add the following line into the getDealerDecision method.

```fsharp
let score = cards |> calculateScore |> printScore
```

Change the print line to be

```fsharp
printfn "Dealer Cards: %s (%s)" dealerCards score
```

</details>

#### Step 4: Run your game to check the result

Use the same method you used for the previous task.

## Feature: Add support for blackjack

#### Step 1: Add blackjack as a player state

Add `| Blackjack ` as a new line on the PlayerState type.  This tells the compiler that we're adding a new option for the type and we'll need to handle this option anywhere we use the type.

#### Step 2: Handle state in getRoundResult

Add the following to the match statement in the getRoundResult method.

```fsharp
| Blackjack, Blackjack -> Push(player)
| _, Blackjack -> Won(player)
```

The match statement looks at the dealer's state and the player's and then compares them to work out the result for the player.  It may help to look at the whole method for a moment before considering this snippet.  

The first line in the snippet above matches when both the dealer and the player have blackjack which results in a draw (known as a push in Blackjack).

The `_` in the second line indicates any dealer status so if they player has blackjack then they will beat the dealer.  Since cases in a match statement are always matched in order then we know that the dealer doesn't have Blackjack as if they did the first line would have been executed. This means that it's ok to assume the dealer has lost if the player has Blackjack and the first line in the above snippet has not been matched.

#### Step 3: Add a function to detect blackjack (2 cards, one of which is an ace and the other is picture)

Add the following below the card type definition.

```fsharp
let isPictureRank rank =
    match rank with
    | King | Queen | Jack -> true
    | _ -> false

let isBlackjack cards =
    match List.map getRank cards with
    | [Ace ; other] when isPictureRank other -> true
    | [other ; Ace] when isPictureRank other -> true
    | _ -> false
```

The isPictureRank function should be fairly intuitive to understand but the match statement in isBlackJack might be somewhat confusing. There are 2 points that might need explanation.  The first is that the `List.map getRank cards` just runs the getRank function we wrote earlier over the players cards and returns a list of cardRanks from the list of cards.

The second is the fact there are 2 cases in the match statement that are vey similar.  This is because we're specifying the the entire list we want to match in the match statement and ordering is important so we have to consider both possible orders.


#### Step 4: Add code to processPlayer to detect blackjack.

Replace the contents of the | Playing(cards) pattern in the processPlayer method with the following

```fsharp
if isBlackjack cards then
    { player with State = Blackjack }, deck
else
    match makeDecision cards visibleDealerCard with
        | Hit ->
            printfn "%s Hits" player.Name
            let card,remaining = draw deck
            let newPlayerCards = card::cards
            let scores = calculateScore newPlayerCards
            if isNotBust scores then
                processPlayer  visibleDealerCard makeDecision remaining {player with State = Playing(newPlayerCards) }
            else
                printfn "Bust! %s" (newPlayerCards |> printCards)
                { player with State = Bust(newPlayerCards) }, remaining
        | Stand ->
            let finalScore = cards |> calculateScore |> getBestNonBustScore
            printfn "%s stands at %i" player.Name finalScore
            { player with State = Stood(finalScore) }, deck
```

Although this is a lot of code the only real change from the existing code is to return immediately if the player has Blackjack rather than asking them if they want another card.

#### Step 5: Test process player in FSharp Interactive to see if blackjack works

Send file to FSharp Interactive first or alternatively run this code (without the semi colons) in your main method.

```fsharp
 let p = { Name = "Richard"; State = Playing([Ace,Heart;King,Diamonds]) };;
 processPlayer (Two,Clubs) getPlayerDecision [] p;;
```

The first line here defines a player type.  This may be somewhat confusing if you've used C# as there's no explicit type passed here but the compiler automatically infers the type of Player from the names of the properties being initialised.

#### Step 6: Test getRoundResult using FSharp Interactive

Send file to FSharp Interactive first or alternatively run this code (without the semi colons) in your main method.

```fsharp
 getRoundResult { Name="Dealer";State=Blackjack} {Name="Richard";State=Stood(21)};;
```

This tests the circumstance of the Dealer having blackjack and the player having stood on 21.  If everything is working then this should result in the player losing.

## Extension exercises

If you've finished these exercises here are some others you can try on your own.

* Add ability to place a bet. Bets must be placed before cards are dealt. If you win double your stake, if you get blackjack you get 1.5 x your initial stake.
* Refactor the code to make more illegal states unrepresentable e.g. refactor types so that the processPlayer method doesn't ever throw exceptions.