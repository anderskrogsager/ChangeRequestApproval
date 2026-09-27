# Kodestruktur og fælles konventioner

## Formål

Dette dokument fastlægger de fælles aftaler, der gør koden hurtig at finde rundt i, læse og reviewe. Det gælder især grænser mellem ansvar, navngivning, kommentarer og teststruktur.

Konventionerne er ikke et forsøg på at forudsige hver lokal variabel. De gør fælles og gentagne valg ens, så læseren ikke skal gætte deres betydning fra fil til fil.

## Find ansvar hurtigt

| Hvis læseren vil finde ... | Start her |
|---|---|
| En regel om status eller validering | `Domain/ChangeRequest.cs` |
| En statusværdi | `Domain/ChangeRequestStatus.cs` |
| JSON-input eller JSON-output | `Api/` og API-kontrakten |
| En URL og dens HTTP-svar | endpointregistreringen i `Program.cs` |
| Lagring og databaseopsætning | `Persistence/ChangeRequestDbContext.cs` og opstartskonfiguration |
| Brugerfladens handling eller visning | `Frontend/` som kildekode; ikke den genererede fil i `wwwroot/` |
| En regeltest | den tilsvarende `...Tests.cs`-fil og sporbarhedstabellen |
| Et samlet HTTP-forløb | workflowtestene og API-kontrakten |

Hvis et ansvar ikke kan placeres i denne oversigt, skal teamet enten vælge en tydelig placering eller opdatere oversigten. Kode må ikke blive liggende i en vilkårlig mappe, blot fordi den ikke umiddelbart passer.

## Fil- og typeansvar

| Område | Regler |
|---|---|
| `Domain/` | Indeholder domæneklasser og status-/værdi-typer. Ingen afhængighed af HTTP, JSON, EF Core eller browser. |
| `Api/` | Indeholder typer, som former HTTP-kontrakten. En request-type repræsenterer input til én navngiven API-handling. |
| `Persistence/` | Indeholder EF Core-konfiguration og dataadgangsrelaterede typer. Ingen workflowregler. |
| `Frontend/` | Indeholder den redigerbare TypeScript-kilde. Funktioner opdeles efter brugerhandling eller skærm, ikke efter tilfældige tekniske hjælpemidler. |
| `wwwroot/` | Indeholder filer, browseren henter. Genereret JavaScript redigeres ikke manuelt. |
| `Program.cs` | Indeholder applikationsopsætning og endpointregistrering. Hver endpointhandler koordinerer ét API-kald; kompleks, genbrugelig logik flyttes til den ansvarlige type. |
| `tests/` | Tests organiseres efter formål: domæneregler, HTTP-workflows og test-infrastruktur. |

## Navngivning

Ordlisten er den autoritative kilde for domæneord. Disse ekstra regler gælder i kode:

| Situation | Aftale |
|---|---|
| Domæneobjekt | Brug `changeRequest`, ikke `request`, `item`, `entity` eller `model`, når flere slags objekter er i scope. |
| HTTP-input | Brug et præcist navn som `createChangeRequestRequest` eller `rejectChangeRequestRequest`. Brug kun `request`, hvis endpointets parameterliste gør betydningen utvetydig. |
| HTTP-objekter | Brug `httpRequest`, `httpResponse` eller typens konkrete navn, når de optræder sammen med API-input. |
| Identitet | Brug `changeRequestId`, når flere id’er kan optræde; et lokalt `id` er kun acceptabelt i en kort, entydig endpointhandler. |
| Resultat | Undgå `result`, medmindre typen og betydningen er tydelig i umiddelbar nærhed. Navngiv i stedet det, der faktisk er returneret eller beregnet. |
| Boolean | Start med et udsagn eller spørgsmål, fx `isDraft`, `hasReviewComment` eller `canSubmitForReview`. |
| Samlinger | Brug flertal, fx `changeRequests`. |
| Asynkrone C#-metoder | Brug endelsen `Async`, når metoden returnerer `Task` eller `Task<T>`. |

Brug eksplicit type, når typen bærer vigtig betydning for læseren. `var` er passende, når typen er åbenlys i højre side og det ikke skjuler en væsentlig abstraktion. Eksempel:

```csharp
var changeRequest = new ChangeRequest();
ChangeRequestStatus status = changeRequest.Status;
```

## Metodenavne og ændringer af tilstand

- Metoder, der ændrer en `ChangeRequest`, bruger et præcist verbum fra ordlisten: `UpdateDetails`, `SubmitForReview`, `Approve` eller `Reject`.
- En ændrende metode må ikke have et navn, der ligner en læsehandling, fx `GetOrSubmit`.
- En læsemetode må ikke have skjulte ændringer af data eller status.
- Metoder med et fælles begreb må ikke opfinde synonymer. Brug eksempelvis ikke både `Approve` og `Accept` for samme handling.

## Kommentarer og kodeafsnit

Kommentarer skal gøre koden hurtigere at forstå, men ikke gentage det, som et godt navn allerede siger.

| Situation | Aftale |
|---|---|
| Fil med ét klart ansvar | Filnavn, typenavn og korte, navngivne metoder er normalt tilstrækkelige. |
| Fil med flere læsbare opstartsafsnit, fx `Program.cs` | Brug korte sektionskommentarer, der beskriver afsnittets ansvar, fx `// Database and JSON configuration` eller `// Change request endpoints`. |
| Ikke-indlysende valg eller begrænsning | Skriv en kommentar, der forklarer **hvorfor**, og link om muligt til teknisk beslutning eller issue. |
| Offentlig API-type eller metode | Brug XML-kommentar, når den giver API-forbruger eller OpenAPI-dokumentation vigtig betydning, begrænsning eller eksempel. |
| Kommentar der blot omskriver koden | Undlad den. Eksempel: `// Set status to Draft` lige over `Status = ChangeRequestStatus.Draft;`. |
| Midlertidig løsning | Kommentaren skal sige hvorfor den er midlertidig og pege på det issue eller den change request, der skal fjerne den. |

En kommentar må ikke være eneste dokumentation for en fælles regel. Reglen skal også kunne findes i krav, domænemodel og testsporbarhed.

## Struktur i en endpointhandler

En endpointhandler skal så vidt muligt følge denne læseretning:

```text
1. Modtag og navngiv HTTP-input
2. Find relevant ChangeRequest eller returnér 404
3. Kald den ansvarlige domænehandling
4. Håndtér forventet forretningsfejl som HTTP-fejl
5. Gem ændringer, når handlingen lykkes
6. Returnér kontraktens HTTP-svar
```

Det er en læseretning, ikke et krav om kunstige hjælpefunktioner. Hvis handleren bliver så lang, at dens hovedtrin ikke er tydelige, skal teamet først undersøge, om ansvar ligger forkert, før der indføres et nyt lag alene for at forkorte filen.

## Testkode

- En test arrangerer data, udfører én handling og kontrollerer det relevante resultat.
- Testnavnet beskriver scenarie og forventet udfald.
- Testens data skal bruge domæneord, fx `changeRequest` og `reviewComment`.
- Fælles hjælpefunktioner er tilladt, når de fjerner gentagen opsætning uden at skjule den regel, testen skal vise.
- En test må ikke afhænge af en tidligere tests oprettede data eller rækkefølge.

## Review af læsbarhed

Ved review kontrolleres mindst:

1. Kan en læser finde ansvar fra dokumentation til fil og metode uden at gætte?
2. Har samme domænebegreb ét navn på tværs af kode, test, API og dokumentation?
3. Er kommentarer forklaringer af ansvar eller begrundelse frem for gentagelser?
4. Er domænelogik fri af HTTP- og databasekendskab?
5. Er endpointhandlernes hovedtrin synlige?
6. Er den redigerbare TypeScript-kilde tydeligt adskilt fra genereret JavaScript?

## Ændring af en konvention

En udvikler må gerne foreslå en bedre konvention. Den ændres først, når teamet har besluttet den, og den relevante ordliste eller dette dokument er opdateret. Derefter bringes eksisterende berørt kode i overensstemmelse som en bevidst opgave, ikke ved tilfældige enkeltændringer.
