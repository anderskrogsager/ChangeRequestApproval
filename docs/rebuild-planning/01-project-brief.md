# Projektbrief

## Problem

Tekniske ændringer kan have konsekvenser for flere systemer, afhængigheder, risici og drift. Før arbejdet begynder, kan et team have brug for en lille, tydelig proces til at beskrive ændringen og træffe en bevidst beslutning om den.

Projektet modellerer den proces som en **change request**. Det er ikke en opgave i et udviklingsværktøj og ikke et pull request. Det handler om beslutningen før implementering af den tekniske ændring.

## Formål

Change Request Approval er et lille, gennemarbejdet projekt til en jobansøgning. Det skal demonstrere, at en udvikler kan:

- afgrænse et problem før teknologien vælges
- gøre forretningsregler og ansvar tydelige
- omsætte regler til enkel, testbar kode
- vælge den mindste løsning, der løser problemet ordentligt
- forklare tekniske valg, fravalg og tradeoffs

Projektet er ikke et forsøg på at efterligne et komplet enterprise-system.

## Brugere og ansvar

Systemet har to begrebsmæssige roller. Første version har ikke login eller autorisation; rollerne beskriver alene workflowet.

| Rolle | Formål i workflowet |
|---|---|
| Requester | Opretter og udfylder en change request, derefter submitter den til review |
| Reviewer | Godkender eller afviser en submitted request og kan tilføje en kommentar |

I en rigtig løsning ville autentifikation og autorisation afgøre, hvem der må udføre hvilke handlinger. Det er bevidst uden for denne demos scope.

## Den mindste nyttige brugerrejse

```text
Opret draft
    → udfyld eller redigér detaljer
    → submit til review
    → approve eller reject
    → se den endelige beslutning
```

En bruger skal kunne:

1. Oprette en change request som draft.
2. Se en oversigt over oprettede requests.
3. Åbne og redigere en draft.
4. Submitte en komplet draft.
5. Godkende eller afvise en submitted request.
6. Se status, reviewkommentar og reviewtidspunkt bagefter.

De præcise regler, fejlscenarier og statusovergange beskrives senere i planlægningspakken.

## Projektets rammer

| Ramme | Betydning |
|---|---|
| Lille scope | Løsningen skal kunne forstås og forklares på få minutter, ikke simulere en stor organisation. |
| C#/.NET-backend | Projektet skal vise moderne .NET-backend og tydelig domænelogik. |
| Lille TypeScript-frontend | Frontend’en skal vise, at API’et kan bruges i en browser, uden at frontend-arkitekturen bliver hovedprojektet. |
| Relevante tests | Tests prioriterer vigtige regler og centrale brugerforløb frem for maksimal testmængde. |
| Lokal kørsel | En anden udvikler skal kunne starte projektet uden ekstern infrastruktur. |
| Offentligt repository | Kode og udvalgt dokumentation skal kunne læses som et selvstændigt jobansøgningsprojekt. |

## Scopegrænser og bevidste fravalg

Følgende fravalg beskrives, fordi de enten kunne forveksles med projektets formål eller ville ændre løsningens kompleksitet væsentligt.

| Ikke i første version | Hvorfor fravalget er bevidst |
|---|---|
| Git, GitHub eller Azure DevOps-integration | En change request er ikke et pull request og skal kunne forstås uden et bestemt kodehostingsystem. |
| Login, roller og brugerprofiler | Realistisk adgangsstyring kræver flere regler og sikkerhedsovervejelser end demoens workflow behøver. |
| Generisk workflow engine | Der findes kun fire statusser og få tydelige overgange; almindelig domænekode er lettere at læse og forklare. |
| E-mail, notifikationer og audit-log | De er plausible funktioner, men ikke nødvendige for at demonstrere beslutningsworkflowet. |
| Vedhæftninger, avanceret søgning, filtrering og paging | Den lille datamængde og brugerrejse behøver dem ikke. |
| Frontend framework eller designsystem | Den lille grænseflade kan bygges og forstås direkte i TypeScript. |

Dette er ikke en liste over alt, systemet aldrig kan få. Det er de scopegrænser, der har betydning for den aktuelle løsning og de valg, teamet skal kunne begrunde.

## Succes for projektet

Projektet er lykkedes, når en læser kan se og forstå:

- den komplette `Draft → Submitted → Approved / Rejected`-rejse
- hvorfor de centrale regler ligger i `ChangeRequest` og ikke kun i frontend eller HTTP-endpoints
- hvordan frontend, API, lagring og tests samarbejder
- hvorfor løsningen er holdt til én applikation og uden unødige abstraktioner
- hvordan en ny udvikler kan finde krav, kode og tests uden at skulle rekonstruere hele systemet fra fragmenter

## Næste dokument

`02-domain-glossary-and-naming.md` fastlægger det fælles sprog, før vi beskriver de præcise regler og flows.
