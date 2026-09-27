# Teststrategi og manuelle testcases

## Formål

Dette dokument fastlægger, hvordan Change Request Approval skal kontrolleres. Det kobler forretningsregler til konkrete automatiserede og manuelle tests.

Testene skal vise, at systemet overholder den aftalte adfærd. De erstatter ikke review af læsbarhed, sikkerhed eller brugeroplevelse.

Dette dokument beskriver de planlagte testscenarier og manuelle procedurer. Den konkrete gennemgang af, hvor testene er implementeret, samt resultater fra udførte testkørsler registreres i `10-traceability-and-review.md`.

## Testniveauer

| Niveau | Hvad testes | Hvad er ikke formålet | Planlagt værktøj eller form |
|---|---|---|---|
| Unit-test | `ChangeRequest`-klassens regler, dataændringer og ugyldige statusovergange. | HTTP, JSON, database eller browser. | xUnit direkte mod domænemodellen. |
| Workflowtest | Et komplet HTTP-forløb gennem endpoints, JSON-binding, domænemodel og testdatabase. | Det præcise visuelle layout i browseren. | xUnit med ASP.NET Core test-host og separat SQLite in-memory-database. |
| Manuel browsertest | Formularer, navigation, synlige fejl, statusvisning og sammenhængende brugerrejser. | At erstatte unit- og workflowtests. | Fast testprocedure udført i en rigtig browser. |

En workflowtest kaldes her også en **acceptance-test**, fordi den kontrollerer en observerbar brugerhistorie gennem systemets offentlige HTTP-grænseflade. Den er ikke en browserautomatiseringstest.

## Testprincipper

1. Vigtige forretningsregler testes direkte mod `ChangeRequest` og gennem mindst ét relevant HTTP-forløb.
2. En fejltest skal kontrollere både fejlresultatet og at data/status ikke er ændret.
3. Workflowtests bruger en isoleret testdatabase. De må ikke læse eller ændre den lokale SQLite-fil, der bruges under manuel udvikling.
4. Hver test har ét tydeligt formål. Testnavnet skal beskrive scenariet og det forventede resultat.
5. Tests må ikke afhænge af, hvilken rækkefølge de køres i.
6. Testdata skal være læsbare og repræsentere et reelt scenarie; vilkårlige strenge bruges kun, når deres indhold er uden betydning.

## Planlagte unit-tests

| Id | Scenarie | Forventet resultat | Regler |
|---|---|---|---|
| UT-01 | Opret en change request. | Den får nyt id, `CreatedAt` i UTC og status `Draft`. | BR-01, BR-02 |
| UT-02 | Opdater en draft. | De tre indholdsfelter ændres; status forbliver `Draft`. | BR-03 |
| UT-03 | Opdater en submitted request. | Handlingen afvises; eksisterende data bevares. | BR-03 |
| UT-04 | Send en komplet draft til review. | Status ændres til `Submitted`. | BR-05, BR-06 |
| UT-05 | Send en draft med manglende Title til review. | Handlingen afvises; status forbliver `Draft`. | BR-06 |
| UT-06 | Send en draft med manglende Description til review. | Handlingen afvises; status forbliver `Draft`. | BR-06 |
| UT-07 | Send en draft med manglende Technical impact til review. | Handlingen afvises; status forbliver `Draft`. | BR-06 |
| UT-08 | Send en draft med whitespace i et indholdsfelt til review. | Handlingen afvises; status forbliver `Draft`. | BR-06 |
| UT-09 | Send en allerede submitted request til review igen. | Handlingen afvises; status forbliver `Submitted`. | BR-05 |
| UT-10 | Approve en submitted request uden kommentar. | Status bliver `Approved`; `ReviewedAt` sættes; kommentar er fraværende. | BR-07, BR-08 |
| UT-11 | Approve en submitted request med kommentar. | Status bliver `Approved`; kommentar og `ReviewedAt` gemmes. | BR-08 |
| UT-12 | Approve en draft. | Handlingen afvises; status forbliver `Draft`. | BR-07 |
| UT-13 | Reject en submitted request med begrundelse. | Status bliver `Rejected`; kommentar og `ReviewedAt` gemmes. | BR-07, BR-09 |
| UT-14 | Reject en submitted request uden begrundelse. | Handlingen afvises; status forbliver `Submitted`; `ReviewedAt` er fortsat fraværende. | BR-09 |
| UT-15 | Reject en submitted request med whitespace som begrundelse. | Handlingen afvises; status forbliver `Submitted`. | BR-09 |
| UT-16 | Udfør en workflowhandling på `Approved`. | Handlingen afvises; data og status bevares. | BR-10 |
| UT-17 | Udfør en workflowhandling på `Rejected`. | Handlingen afvises; data og status bevares. | BR-10 |

## Planlagte workflowtests

| Id | Scenarie | Kontrollerer | Regler/kontrakt |
|---|---|---|---|
| AT-01 | Klient opretter tom draft, opdaterer den med komplet indhold, sender den til review og henter den igen. | Samspillet mellem JSON, endpoints, domænemodel, lagring og svar. | FR-01, FR-03, FR-04, FR-05; BR-01–BR-06 |
| AT-02 | Klient approver en submitted request med kommentar. | Approve-endpoint, response-data og final status. | FR-06, BR-07, BR-08 |
| AT-03 | Klient rejecter en submitted request med begrundelse. | Reject-endpoint, response-data og final status. | FR-07, BR-07, BR-09 |
| AT-04 | Klient forsøger at rejecte uden begrundelse. | `400 Bad Request`, fejl-body og uændret `Submitted`-status. | BR-09, API-kontrakt |
| AT-05 | Klient forsøger at opdatere en submitted request. | `400 Bad Request`, fejl-body og uændrede indholdsfelter. | BR-03, API-kontrakt |
| AT-06 | Oversigten indeholder oprettede requests i stabil rækkefølge, nyeste først. | Liste-endpointets data og sortering. | FR-02, API-kontrakt |
| AT-07 | Klient bruger ukendt eller ugyldigt id. | `404 Not Found`. | API-kontrakt |
| AT-08 | Klient sender ufuldstændig draft til review. | `400 Bad Request`, fejl-body og fortsat `Draft`. | BR-06, API-kontrakt |

## Manuelle browsertestcases

### MT-01: Opret og se en tom draft

**Forudsætning:** Appen kører, og oversigten er åben.

1. Lad Title, Description og Technical impact være tomme.
2. Vælg handlingen for at oprette/gemme en draft.
3. Åbn den oprettede request fra oversigten.

**Forventet resultat:** Requesten findes i oversigten og viser status `Draft`. Det er muligt at åbne detaljevisningen og redigere de tre indholdsfelter.

### MT-02: Forsøg at sende ufuldstændig draft til review

**Forudsætning:** En draft er åben. Dens gemte Title har en kendt værdi, som kan ændres i testen.

1. Ændr Title til `Draft retained after failed review`.
2. Lad Technical impact være tomt eller kun whitespace.
3. Vælg “Send til review”.
4. Læs fejlbeskeden.
5. Genindlæs browseren eller gå tilbage til oversigten og åbn requesten igen.

**Forventet resultat:** En forståelig fejl vises. Status er stadig `Draft`, og indholdsfelterne kan fortsat redigeres. Den ændrede Title og det tomme eller whitespace-baserede Technical impact er gemt på draften. Det beviser, at browseren gemmer de aktuelle felter før den forsøger at sende til review.

### MT-03: Send komplet draft til review

**Forudsætning:** En draft er åben.

1. Indtast ikke-blank tekst i Title, Description og Technical impact.
2. Vælg “Send til review”. Browseren gemmer de aktuelle indholdsfelter som en del af handlingen.

**Forventet resultat:** Status vises som `Submitted`. Redigeringsfelter og mulighed for at sende til review igen er ikke tilgængelige. Reviewmuligheder vises.

### MT-04: Reject uden begrundelse

**Forudsætning:** En request med status `Submitted` er åben.

1. Lad reviewkommentaren være tom.
2. Vælg reject.

**Forventet resultat:** En forståelig fejl vises. Status er fortsat `Submitted`, og reviewmulighederne er stadig tilgængelige.

### MT-05: Reject med begrundelse

**Forudsætning:** En request med status `Submitted` er åben.

1. Indtast en konkret reviewbegrundelse.
2. Vælg reject.

**Forventet resultat:** Status vises som `Rejected`. Begrundelsen og reviewtidspunktet vises. Der er ikke længere redigerings- eller reviewhandlinger.

### MT-06: Approve med valgfri kommentar

**Forudsætning:** En anden request med status `Submitted` er åben.

1. Indtast eventuelt en reviewkommentar.
2. Vælg approve.

**Forventet resultat:** Status vises som `Approved`. Eventuel kommentar og reviewtidspunkt vises. Der er ikke længere redigerings- eller reviewhandlinger.

### MT-07: Kontrollér oversigt og genindlæsning

**Forudsætning:** Mindst to requests er oprettet, herunder én final request.

1. Gå tilbage til oversigten.
2. Kontrollér, at nyeste requests står først.
3. Genindlæs browseren.
4. Åbn den final request igen.

**Forventet resultat:** Oversigten og de gemte statusser er bevaret. Den finale request viser fortsat status, reviewkommentar og reviewtidspunkt korrekt.

## Udeladt i denne version

- Automatisk browserautomatisering med eksempelvis Playwright eller Selenium.
- Belastnings-, sikkerheds- og tilgængelighedstest.
- Test af login og adgangskontrol, da systemet ikke har disse funktioner.

Disse fravalg betyder ikke, at områderne er uvigtige. De ligger uden for denne lille apps aftalte scope og skal genvurderes, hvis systemet får flere brugere, følsomme data eller en produktionsdriftsrolle.
