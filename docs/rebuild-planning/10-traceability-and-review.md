# Sporbarhed og review

## Formål

Sporbarhed gør det muligt at følge en aftalt ændring hele vejen fra krav til kode og test. Dokumentet skal bruges ved planlægning, pull request-review og afsluttende kvalitetssikring.

Det forhindrer ikke fejl af sig selv. Det giver revieweren en konkret liste over, hvad der skal findes og kontrolleres, i stedet for at reviewet alene afhænger af hukommelse eller tilfældig læsning af kode.

De planlagte testscenarier og de manuelle testprocedurer er beskrevet i `08-test-strategy-and-manual-test-cases.md`. Dette dokument registrerer deres konkrete kodebeviser og testresultater.

## To reviewtidspunkter

| Tidspunkt | Grundlag | Det der godkendes |
|---|---|---|
| Change request-review | Krav, regler, flows, API-kontrakt, tekniske beslutninger og testplan. | Den beskrevne ændring må implementeres. |
| Pull request-review | Den godkendte change request samt den konkrete kodeændring og dens tests. | Implementeringen stemmer med den godkendte ændring og er klar til at flettes. |

En pull request må gerne afdække en ny observation. Hvis den kun angår en lokal implementeringsdetalje inden for de godkendte aftaler, kan den behandles i pull request-reviewet. Hvis den ændrer en fælles regel, kontrakt eller teknisk beslutning, må pull requesten ikke blot godkendes efter en dokumentopdatering: den tilknyttede change request skal opdateres og godkendes igen. Er ændringen uden for den oprindelige change requests scope, oprettes en ny change request.

## Sporbarhedsstatus

Dokumentet starter med en forberedt reviewstruktur. Før implementering udfyldes kolonnerne med forventet kode og planlagte tests. Under review udfyldes faktiske fund og status på den samme række. Strukturen ændres, når reviewet afdækker et reelt behov for et nyt kontrolpunkt; den ændres ikke blot, fordi en række skal udfyldes.

| Status | Betydning |
|---|---|
| `Planned` | Aftalt, men endnu ikke kontrolleret mod implementeringen. |
| `Matches` | De fundne kode- og testbeviser følger aftalen på dette punkt. |
| `Partial` | Koden følger aftalen, men testbeviset er begrænset; eller kun en del er gennemgået. |
| `Missing direct test` | Koden følger aftalen ved læsning, men der findes ikke en målrettet test. |
| `Manually verified` | Relevant manuel testcase er gennemført og bestået. |
| `Deviation` | Implementering og aftale afviger; der skal træffes og dokumenteres en beslutning. |

## Krav- og regelsporbarhed

| Id | Krav eller regel | Forventet kodeansvar | Planlagte tests | Faktisk kodefund | Faktisk testfund | Status |
|---|---|---|---|---|---|---|
| FR-01 | Oprette change request. | `ChangeRequest`; create-endpoint; oprettelsesformular. | UT-01, AT-01, MT-01 | `ChangeRequest`-constructor, create-endpoint og `app.ts`: `renderCreateForm`/`handleCreateSubmit` er gennemgået. | Unit-test, workflowtest og MT-01 består. | Matches |
| FR-02 | Se oversigt, nyeste først. | liste-endpoint; oversigtsvisning. | AT-06, MT-07 | Liste-endpointet sorterer stabilt; `app.ts`: `renderChangeRequests` viser oversigten. | Workflowtest og MT-07 består. | Matches |
| FR-03 | Se én request. | get-by-id-endpoint; detaljevisning. | AT-01, MT-01 | Get-by-id-endpointet og `app.ts`: `showChangeRequest`/`renderChangeRequestDetail` er gennemgået. | Workflowtest og MT-01 består. | Matches |
| FR-04 | Redigere draft. | `ChangeRequest.UpdateDetails`; update-endpoint; draftformular. | UT-02, UT-03, AT-01, AT-05 | `UpdateDetails`, PUT-handler og `app.ts`: `renderDraftEditForm`/`handleDraftUpdateSubmit` er gennemgået. | Relevante unit- og workflowtests samt MT-01 består. | Matches |
| FR-05 | Sende komplet draft til review. | `ChangeRequest.SubmitForReview`; workflow-endpoint; draftvisning. | UT-04–UT-09, AT-01, AT-08, MT-02, MT-03 | `SubmitForReview`, POST `/submit-for-review` og `app.ts`: `handleSendToReview` er gennemgået. Browseren gemmer de aktuelle felter med PUT før POST til submit-endpointet. | Relevante unit- og workflowtests samt MT-03 består. MT-02 skal gentages med det nye kontrolpunkt om gemte felter. | Partial |
| FR-06 | Approve submitted request. | `ChangeRequest.Approve`; approve-endpoint; reviewvisning. | UT-10–UT-12, AT-02, MT-06 | `Approve`, POST `/approve` og `app.ts`: `renderReviewForm`/`handleReviewSubmit` er gennemgået. | Relevante unit- og workflowtests samt MT-06 består. | Matches |
| FR-07 | Reject submitted request. | `ChangeRequest.Reject`; reject-endpoint; reviewvisning. | UT-13–UT-15, AT-03, AT-04, MT-04, MT-05 | `Reject`, POST `/reject` og `app.ts`: `renderReviewForm`/`handleReviewSubmit` er gennemgået. | Relevante unit- og workflowtests samt MT-04 og MT-05 består. | Matches |
| BR-01 | Ny request starter `Draft`. | `ChangeRequest` ved oprettelse. | UT-01, AT-01, MT-01 | Constructoren sætter `Status = Draft`. | `New_change_request_starts_as_draft`. | Matches |
| BR-02 | Systemet sætter id og oprettelsestid. | `ChangeRequest` ved oprettelse. | UT-01, AT-01 | Constructoren bruger `Guid.NewGuid()` og `DateTimeOffset.UtcNow`. | `New_change_request_starts_as_draft_with_an_id_and_utc_creation_time` kontrollerer id og UTC-offset; workflowtesten læser oprettelsessvaret. | Matches |
| BR-03 | Kun draft kan opdateres. | `ChangeRequest.UpdateDetails`. | UT-02, UT-03, AT-05 | `UpdateDetails` afviser enhver status undtagen `Draft`. | Unit- og workflowtests kontrollerer både afvisning og uændrede detaljer; final-status-tilfælde testes under BR-10. | Matches |
| BR-04 | Draft må være ufuldstændig. | create- og update-handling. | AT-01, MT-01 | Constructoren og `UpdateDetails` tillader tomme indholdsfelter. | `Draft_can_be_incomplete`, workflowtest og MT-01 beviser den tilladte tomme draft. | Matches |
| BR-05 | Kun draft kan sendes til review. | `ChangeRequest.SubmitForReview`. | UT-04, UT-09, AT-01 | `SubmitForReview` afviser enhver status undtagen `Draft`. | Positive, gentagne og finale statusovergange testes direkte og gennem HTTP. | Matches |
| BR-06 | Indhold skal være komplet ved send til review. | `ChangeRequest.SubmitForReview`. | UT-05–UT-08, AT-08, MT-02, MT-03 | `SubmitForReview` bruger `IsNullOrWhiteSpace` på alle tre felter. | Teorien tester tomt og whitespace i hvert indholdsfelt; workflowtest og manuelle tests kontrollerer HTTP- og browserforløb. | Matches |
| BR-07 | Kun submitted kan reviewes. | `ChangeRequest.Approve`; `ChangeRequest.Reject`. | UT-12, UT-13, AT-02, AT-03 | `Approve` og `Reject` afviser enhver anden status. | Draft kan ikke approve/rejecte; final-status-testene kontrollerer også afvisning. | Matches |
| BR-08 | Approve sætter final status og reviewtidspunkt. | `ChangeRequest.Approve`. | UT-10, UT-11, AT-02, MT-06 | `Approve` sætter `Approved`, eventuel kommentar og `DateTimeOffset.UtcNow`. | Begge approve-scenarier kontrollerer status, kommentar, tidspunkt og UTC-offset. | Matches |
| BR-09 | Reject kræver begrundelse og sætter reviewtidspunkt. | `ChangeRequest.Reject`. | UT-13–UT-15, AT-03, AT-04, MT-04, MT-05 | `Reject` bruger `IsNullOrWhiteSpace`, sætter `Rejected`, kommentar og `DateTimeOffset.UtcNow`. | Gyldig, tom og whitespace-kommentar testes; både succes- og fejlscenarier kontrollerer forventet status/tidspunkt. | Matches |
| BR-10 | Final requests er låste. | Beskyttelse i `ChangeRequest`-handlinger. | UT-16, UT-17, MT-05, MT-06 | Alle fire handlinger afviser `Approved` og `Rejected`. | Begge final-status-tests forsøger update, send til review, approve og reject samt kontrollerer, at detaljer, status, kommentar og reviewtidspunkt bevares. | Matches |

## Brugerrejsesporbarhed

Denne tabel samler den vandrette kontrol af et helt brugerforløb. Den er ikke en gentagelse af regelsporing: en brugerrejse går på tværs af brugerflade, endpoint, domænemodel og lagring.

| Id | Brugerrejse | Forventet brugerflade og handling | Forventet serverforløb | Planlagt evidens | Faktisk fund | Status |
|---|---|---|---|---|---|---|
| UJ-01 | Opret og se en draft. | Oprettelsesformular; oversigt og detaljevisning viser den oprettede `Draft`. | `POST /change-requests` opretter; `GET` viser requesten. | AT-01, MT-01 | `Program.cs`: create- og get-handlere. `Frontend/app.ts`: `renderCreateForm`, `handleCreateSubmit`, `renderChangeRequests` og `showChangeRequest`. | AT-01 og MT-01 består. | Matches |
| UJ-02 | Rediger en draft. | Detailvisning af `Draft` viser redigerbare indholdsfelter og gemmehandling. | `PUT /change-requests/{id}` kalder `UpdateDetails` og gemmer. | UT-02, UT-03, AT-01, AT-05, MT-01 | `ChangeRequest.UpdateDetails`, PUT-handleren samt `app.ts`: `renderDraftEditForm` og `handleDraftUpdateSubmit`. Formularen vises kun ved `Draft`. | Relevante unit- og workflowtests samt MT-01 består. | Matches |
| UJ-03 | Send en draft til review. | Detailvisning af `Draft` tilbyder handlingen; fejl vises ved ufuldstændigt indhold; `Submitted` vises efter succes. | Browseren gemmer aktuelle felter; POST `/submit-for-review` kalder derefter `SubmitForReview`. | UT-04–UT-09, AT-01, AT-08, MT-02, MT-03 | `ChangeRequest.SubmitForReview`, POST `/submit-for-review` og `app.ts`: `handleSendToReview`. Browseren gemmer først felterne og kalder derefter endpointet; den viser serverfejl. | Relevante unit- og workflowtests samt MT-03 består. MT-02 skal gentages med det nye kontrolpunkt om gemte felter. | Partial |
| UJ-04 | Approve en submitted request. | Detailvisning af `Submitted` tilbyder valgfri kommentar og approve; final visning viser resultat. | Approve-endpoint kalder `Approve`, gemmer og returnerer final request. | UT-10–UT-12, AT-02, MT-06 | `ChangeRequest.Approve`, POST `/approve`, workflowtesten `User_can_approve_a_submitted_change_request` samt `app.ts`: `renderReviewForm` og `handleReviewSubmit`. Reviewformularen vises kun ved `Submitted`; final status viser ingen handlinger. | AT-02 og MT-06 består. | Matches |
| UJ-05 | Reject en submitted request. | Detailvisning af `Submitted` kræver begrundelse; fejl vises ved tom begrundelse; final visning viser resultat. | Reject-endpoint kalder `Reject`, gemmer ved succes og returnerer fejl ved blank begrundelse. | UT-13–UT-15, AT-03, AT-04, MT-04, MT-05 | `ChangeRequest.Reject`, POST `/reject` og `app.ts`: `renderReviewForm` og `handleReviewSubmit`. Serverfejl vises i formularen; final status viser ingen handlinger. | AT-03, AT-04, MT-04 og MT-05 består. | Matches |

## Klassens form og ansvar

| Planlagt modelpunkt | Forventet implementering | Faktisk fund | Status |
|---|---|---|---|
| `ChangeRequest` har de aftalte datafelter. | `Id`, `Title`, `Description`, `TechnicalImpact`, `Status`, `CreatedAt`, `ReviewComment` og `ReviewedAt`. | Klassen har alle otte felter. | Matches |
| Felter kan ikke ændres frit udefra. | Ingen offentlig setter for modellens data eller status. | Alle egenskaber har `private set`. | Matches |
| Klassen ejer status- og valideringsreglerne. | Guards og validering ligger i `UpdateDetails`, `SubmitForReview`, `Approve` og `Reject`. | Guards og validering ligger i `UpdateDetails`, `SubmitForReview`, `Approve` og `Reject`. | Matches |
| Udeladte funktioner findes ikke. | Ingen genåbning, sletning, identitet eller historik. | Klassen indeholder ingen sådanne felter eller handlinger. | Matches |
| Metoden hedder `SubmitForReview`. | Offentlig metode med dette navn. | Den faktiske metode hedder `SubmitForReview`. | Matches |

## Persistence og opstartskontrol

| Planlagt kontrolpunkt | Forventet implementering | Faktisk fund | Status |
|---|---|---|---|
| Domænemodellen kan læses og gemmes. | `ChangeRequestDbContext` eksponerer et `DbSet<ChangeRequest>`. | `ChangeRequestDbContext.ChangeRequests` returnerer `Set<ChangeRequest>()`. | Matches |
| Lokal app bruger SQLite fra konfiguration. | `ChangeRequestDbContext` registreres med `UseSqlite` og den navngivne connection string `ChangeRequestDatabase`. | `Program.cs` læser connection stringen og konfigurerer `UseSqlite`; `appsettings.json` peger på `change-requests.db`. | Matches |
| Hver HTTP-request får en scoped DbContext. | `AddDbContext<ChangeRequestDbContext>` bruges; der oprettes ikke en singleton. | `Program.cs` registrerer med `AddDbContext`. ASP.NET Core bruger scoped levetid som standard for denne registrering. | Matches |
| Databasen initialiseres ved opstart. | En kortvarig service scope henter `ChangeRequestDbContext` og kalder databaseinitialisering. | `Program.cs` opretter scope, henter DbContext og kalder `Database.EnsureCreated()`. | Matches |
| Lokale data kommer ikke i Git. | Databasefil og SQLite WAL-filer ignoreres. | `.gitignore` indeholder `*.db`, `*.db-shm` og `*.db-wal`. | Matches |
| Workflowtests bruger ikke udviklerens databasefil. | Test-host erstatter den lokale SQLite-konfiguration med en separat SQLite in-memory-database. | `ChangeRequestApiFactory` fjerner den oprindelige DbContext-registrering og bruger en åben `Data Source=:memory:`-forbindelse. Factoryen deles af testklassen, men hver test opretter og finder sine egne requests via id. | Matches |

Den sidste række beviser adskillelse fra udviklerens databasefil. Workflowtestene deler én database inden for testklassen, men de aktuelle tests afhænger ikke af en tom database eller af hinandens oprettede id’er. Det opfylder kravet om uafhængig testadfærd, men er en bevidst enklere opsætning end at oprette en helt ny database pr. test.

## API-kontraktsporbarhed

| Kontraktoperation | Forventet endpointansvar | Planlagt test | Faktisk kodefund | Faktisk testfund | Status |
|---|---|---|---|---|---|
| `POST /change-requests` | Opret request, returnér `201 Created` og `Location`. | AT-01 | Opretter `ChangeRequest`, gemmer den og bruger `Results.Created` med dens URL. | `User_can_create_update_send_to_review_and_retrieve_a_change_request` kontrollerer `201 Created`, response-data og `Location`. | Matches |
| `GET /change-requests` | Returnér stabilt sorteret oversigt. | AT-06 | Henter requests og sorterer efter `CreatedAt` faldende, derefter `Id` faldende. | `User_can_view_change_requests_with_newest_first` kontrollerer, at response-arrayet følger denne sortering. | Matches |
| `GET /change-requests/{id}` | Returnér én request eller `404 Not Found`. | AT-01, AT-07 | `FindAsync(id)` returnerer `200 OK` eller `404 NotFound`; route constraint afviser ugyldigt GUID. | Positive hentninger testes, og `User_gets_not_found_for_an_unknown_or_invalid_change_request_id` kontrollerer begge 404-scenarier. | Matches |
| `PUT /change-requests/{id}` | Opdater detaljer gennem domænemodellen. | AT-01, AT-05 | Finder request, kalder `UpdateDetails`, gemmer ved succes og mapper domænefejl til `{ error }` med `400`. | Workflowtests kontrollerer både opdatering og at en afvist opdatering bevarer de oprindelige detaljer. | Matches |
| `POST /change-requests/{id}/submit-for-review` | Kald `SubmitForReview`; returnér opdateret request eller forretningsfejl. | AT-01, AT-08 | POST `/submit-for-review` kalder `SubmitForReview` og bruger fælles fejlformat. | Positive og ufuldstændige HTTP-forløb testes; fejlscenariet kontrollerer fortsat `Draft`. | Matches |
| `POST /change-requests/{id}/approve` | Kald `Approve`; returnér opdateret request eller forretningsfejl. | AT-02 | Finder request, kalder `Approve` med valgfri kommentar, gemmer ved succes og mapper domænefejl til `{ error }` med `400`. | `User_can_approve_a_submitted_change_request` kontrollerer `200`, status, kommentar, reviewtidspunkt og senere hentning. | Matches |
| `POST /change-requests/{id}/reject` | Kald `Reject`; returnér opdateret request eller forretningsfejl. | AT-03, AT-04 | Finder request, kalder `Reject`, gemmer ved succes og mapper domænefejl til `{ error }` med `400`. | Positive og afviste rejectforløb testes; fejlscenariet kontrollerer fortsat `Submitted` efterfølgende. | Matches |

### Fælles API-kontroller

| Kontraktpunkt | Forventet implementering | Faktisk fund | Status |
|---|---|---|---|
| JSON bruger `camelCase`, og status returneres som tekst. | ASP.NET Core JSON-konfiguration med string-enum-konvertering. | `ConfigureHttpJsonOptions` tilføjer `JsonStringEnumConverter`; webstandardens JSON-navngivning giver `camelCase`. | Matches |
| Et response indeholder de aftalte ChangeRequest-felter. | Endpoints returnerer change requesten som JSON. | Endpoints returnerer `ChangeRequest` direkte; dens læsbare egenskaber svarer til kontraktens felter. | Workflowtestens `ChangeRequestResponse` læser alle otte felter, og AT-01 kontrollerer værdier gennem create, update, send til review og hentning. | Matches |
| Forretningsfejl bruger fælles fejlformat. | Ugyldige domænehandlinger returnerer `400` med `{ "error": "..." }`. | Update-, send-til-review-, approve- og reject-handlere catcher `InvalidOperationException` og returnerer dette format. | Update-, send-til-review- og reject-workflowtests læser og kontrollerer `ErrorResponse`. | Matches |
| Klienten kan ikke sætte systemstyrede felter. | Create- og update-requesttyper har kun tre indholdsfelter. | `CreateChangeRequestRequest` og `UpdateChangeRequestRequest` indeholder kun Title, Description og TechnicalImpact. | Matches |

## Tekniske beslutninger til review

| Id | Reviewspørgsmål | Faktisk fund | Status |
|---|---|---|---|
| TD-01 | Er brugerfladefiler og backend stadig udgivet fra én applikation uden ubegrundet ekstra driftskompleksitet? | `Program.cs` konfigurerer både statiske browserfiler og HTTP-endpoints i samme `WebApplication`. | Matches |
| TD-02 | Håndhæver `ChangeRequest` status- og indholdsreglerne, eller er væsentlige regler flyttet ud i endpoints eller frontend? | Endpoints kalder `ChangeRequest`-metoder for update, send til review, approve og reject. Frontenden sender handlingerne, men afgør ikke deres gyldighed. | Matches |
| TD-03 | Bruger lokal udvikling SQLite, og er testdatabasen isoleret? | Lokal app bruger SQLite-fil; test-factoryen erstatter den med SQLite in-memory. Tests deler factoryens database, men forudsætter ikke dens tidligere data. | Matches |
| TD-04 | Er der indført ekstra dataadgangslag? Hvis ja, hvilket selvstændigt ansvar eller problem begrunder det? | Endpoints bruger `ChangeRequestDbContext` direkte. Der findes intet repository-lag. | Matches |
| TD-05 | Er TypeScript-brugerfladen fortsat overskuelig uden unødvendigt framework eller global tilstand? | `Frontend/app.ts` bruger TypeScript, navngivne funktioner og browserens DOM-API; der er ingen frameworkafhængighed eller global mutérbar applikationstilstand. TypeScript bygger uden fejl med `npm run build`. | Matches |
| TD-06 | Dækker unit-tests reglerne, og dækker workflowtests HTTP-samspillet? | Unit-tests dækker domænereglerne; workflowtests dækker de aftalte HTTP-forløb, fejl og kontraktpunkter. | Matches |

## Pull request-reviewprocedure

1. Åbn den linkede change request eller det work item, der angiver formål og accepterede regler.
2. Find de berørte rækker i sporbarhedstabellen.
3. Kontrollér, at den relevante kode findes på de forventede ansvarssteder, eller at en afvigelse er begrundet og godkendt.
4. Læs de relevante automatiserede tests. Kontrollér både positiv og negativ adfærd, når en regel har en afvisningsvej.
5. Kontrollér API-kontrakten, hvis endpoint, JSON eller HTTP-svar er berørt.
6. Kontrollér domæneord og navne mod ordlisten.
7. Vurder, om en manuel testcase eller teknisk beslutning skal opdateres.
8. Kør eller kontrollér build og automatiserede tests.
9. Godkend kun, når afvigelser er løst. En ny fælles beslutning kræver opdatering og fornyet godkendelse af den tilknyttede change request, før pull requesten kan godkendes.

## Registrering af en testkørsel

Selve testcasen er en genbrugelig procedure. En testkørsel er dens resultat for en bestemt version.

| Felt | Eksempel |
|---|---|
| Version eller commit | `a1b2c3d` |
| Miljø | Lokal udvikling / test / staging |
| Udført af | Navn eller initialer |
| Dato | `2026-09-22` |
| Testcase-id | `MT-01`–`MT-07` |
| Resultat | Bestået / fejlet / blokeret |
| Observation eller fejl-id | Kort note eller link |

For dette lille projekt kan en manuel testkørsel registreres som et kort afsnit i pull requesten eller i en release-note. I et større system bør den ligge i et teststyringsværktøj, så tidligere kørsler kan findes igen.

### Seneste automatiske testkørsel

| Felt | Værdi |
|---|---|
| Dato | 2026-09-23 |
| Miljø | Lokal udvikling |
| Kommando | `dotnet test` |
| Resultat | 30 bestået, 0 fejlet, 0 sprunget over |
| Omfang | Domæne- og workflowtests |

### Seneste frontend-build

| Felt | Værdi |
|---|---|
| Dato | 2026-09-23 |
| Miljø | Lokal udvikling |
| Kommando | `npm run build` |
| Resultat | TypeScript kompilerede uden fejl |

### Seneste manuelle testkørsel før den aktuelle navneændring

| Felt | Værdi |
|---|---|
| Dato | 2026-09-23 |
| Miljø | Lokal udvikling i browser |
| Udført af | Anders Krogsager |
| Version eller commit | Arbejdskopi før omdøbning til `SubmitForReview` og `/submit-for-review` |
| Resultat | MT-01 og MT-03–MT-07 bestået; MT-02 skal gentages efter udvidet testprocedure |

| Testcase | Resultat | Kort observation |
|---|---|---|
| MT-01 | Bestået | Tom draft kan oprettes, findes i oversigten og redigeres. |
| MT-02 | Skal gentages | Den tidligere kørsel kontrollerede fejl og status, men ikke at de aktuelle felter blev gemt før den afviste submit-handling. |
| MT-03 | Bestået | Komplet draft kan sendes til review og vises som `Submitted`. |
| MT-04 | Bestået | Reject uden begrundelse viser fejl og bevarer `Submitted`. |
| MT-05 | Bestået | Reject med begrundelse viser `Rejected`, begrundelse og reviewtidspunkt. |
| MT-06 | Bestået | Approve med valgfri kommentar viser `Approved`, eventuel kommentar og reviewtidspunkt. |
| MT-07 | Bestået | Oversigten viser nyeste først, og gemte data bevares efter genindlæsning. |

Den aktuelle TypeScript-kilde og HTTP-rute blev ændret efter denne testkørsel. `npm run build` og workflowtestene beviser den nye rute, men erstatter ikke en manuel test af browseren. MT-01–MT-07 blev derfor udført igen som registreret nedenfor.

### Seneste manuelle testkørsel

| Felt | Værdi |
|---|---|
| Dato | 2026-09-23 |
| Miljø | Lokal udvikling i browser |
| Udført af | Anders Krogsager |
| Version eller commit | Aktuel arbejdskopi efter omdøbning til `SubmitForReview` og `/submit-for-review` |
| Resultat | MT-01–MT-07 bestået |

| Testcase | Resultat | Kort observation |
|---|---|---|
| MT-01 | Bestået | Tom draft kan oprettes, findes i oversigten og redigeres. |
| MT-02 | Bestået | Ufuldstændig draft kan ikke sendes til review; status forbliver `Draft`. |
| MT-03 | Bestået | Komplet draft kan sendes til review og vises som `Submitted`. |
| MT-04 | Bestået | Reject uden begrundelse viser fejl og bevarer `Submitted`. |
| MT-05 | Bestået | Reject med begrundelse viser `Rejected`, begrundelse og reviewtidspunkt. |
| MT-06 | Bestået | Approve med valgfri kommentar viser `Approved`, eventuel kommentar og reviewtidspunkt. |
| MT-07 | Bestået | Oversigten viser nyeste først, og gemte data bevares efter genindlæsning. |

## Afgrænsede opfølgninger

Sektionen er forberedt fra dokumentets oprettelse. Under review tilføjes kun fund, der kræver en beslutning eller en efterfølgende ændring. Tom sektion betyder, at ingen afgrænsede opfølgninger er fundet.

Den første automatiske gennemgangs fund er gennemført: navngivningen er ensrettet, de manglende unit- og workflowtests er tilføjet, og `dotnet test` består med 30 tests.

Før GitHub-klargøring mangler én afgrænset opfølgning:

1. Gentag MT-02 med kontrol af, at browseren gemmer de aktuelle draftfelter før den afviste send-til-review-handling.
