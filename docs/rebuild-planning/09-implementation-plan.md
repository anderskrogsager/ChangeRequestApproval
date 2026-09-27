# Implementeringsplan

## Formål

Denne plan opdeler den aftalte løsning i små implementeringsopgaver. Hver opgave har et tydeligt ansvar, kendte forudsætninger og kontrollerbar afslutning.

Planen skal kunne bruges af én udvikler eller fordeles mellem flere. Den erstatter ikke faglig vurdering ved review, men den skal gøre det synligt, hvad der mangler, og hvilken rækkefølge der er sikker.

## Arbejdsregel for en opgave

Før en opgave markeres færdig, skal udvikleren kunne svare på:

1. Hvilke krav og regler implementerer den?
2. Hvilken kodeenhed ejer ændringen?
3. Hvilke automatiserede tests viser den?
4. Skal en manuel testcase tilføjes eller opdateres?
5. Er API-kontrakt, domænesprog eller teknisk beslutning ændret?

Hvis svaret på punkt 5 er ja, opdateres den relevante beslutning eller kontrakt før eller sammen med koden. Koden må ikke være det eneste sted, hvor et nyt fælles valg findes.

## Implementeringsrækkefølge

| Rækkefølge | Id | Opgave | Afhænger af |
|---:|---|---|---|
| 1 | IP-01 | Opret løsning, webprojekt, testprojekt og lokal udviklingsopsætning. | Projektbeskrivelse |
| 2 | IP-02 | Implementér status-typen og `ChangeRequest`-modellen. | Domæneord, regler, statusmodel |
| 3 | IP-03 | Implementér og kør unit-tests for domænemodellen. | IP-02, teststrategi |
| 4 | IP-04 | Tilføj EF Core, SQLite og databaseinitialisering. | IP-02, arkitektur |
| 5 | IP-05 | Implementér request-/response-typer og read/write-endpoints. | IP-02, IP-04, API-kontrakt |
| 6 | IP-06 | Implementér workflow-endpoints for send til review, approve og reject. | IP-05, API-kontrakt |
| 7 | IP-07 | Implementér workflowtests gennem HTTP. | IP-05, IP-06, teststrategi |
| 8 | IP-08 | Implementér den lille TypeScript-brugerflade. | IP-05, IP-06, API-kontrakt |
| 9 | IP-09 | Udfør manuelle browsertests og ret afvigelser. | IP-08, manuelle testcases |
| 10 | IP-10 | Gennemfør sporbarheds-, læsbarheds- og dokumentationsreview. | Alle forrige opgaver |

## IP-01: Projektgrundlag

**Leverance:** Én ASP.NET Core-webapplikation, ét xUnit-testprojekt, en løsningfil, TypeScript-kompilering og `.gitignore` for build-output, `node_modules` og lokal database.

**Kontrolleres ved:** Projektet bygger, tomme tests kan køres, og browserfiler kan bygges fra TypeScript-kilde.

**Vigtige aftaler:** Lokale udviklingsdata må ikke komme i Git. Konfigurationsværdier, som ikke er hemmelige og er nødvendige for lokal opstart, skal være tydeligt dokumenteret.

## IP-02: Domænemodel

**Leverance:** `ChangeRequestStatus` og `ChangeRequest` med de felter og handlinger, der er besluttet i status- og domænemodellen.

**Arbejdsrækkefølge:**

1. Definér de fire statusværdier.
2. Definér systemstyrede felter og deres oprettelsesværdier.
3. Implementér `UpdateDetails`.
4. Implementér `SubmitForReview` med indholdsvalidering.
5. Implementér `Approve` og `Reject` med reviewregler.
6. Sørg for, at felter ikke kan ændres frit uden om handlingerne.

**Kontrolleres ved:** Unit-tests for både gyldige og ugyldige overgange er grønne.

**Omfang:** Klassen kender ikke HTTP, JSON, database eller browser.

## IP-03: Unit-tests for domænemodellen

**Leverance:** Unit-tests svarende til `UT-01`–`UT-17` i teststrategien.

**Arbejdsform:** For en regel kan testen med fordel skrives først og være rød, før den tilsvarende domænehandling implementeres. Når eksisterende kode ændres, skal testen først vise den ønskede nye regel eller afgrænse den fejl, der rettes.

**Kontrolleres ved:** Alle tests kører isoleret uden database, HTTP-server eller afhængighed af klokkeslæt ud over de forhold, testen selv kontrollerer.

## IP-04: Lagring

**Leverance:** `ChangeRequestDbContext`, SQLite-konfiguration og sikker databaseinitialisering ved opstart.

**Kontrolleres ved:** Appen kan oprette en lokal database, gemme en `ChangeRequest` og læse den igen efter genstart.

**Vigtige aftaler:** `DbContext` oprettes pr. scope via ASP.NET Core DI. En kortvarig opstarts-scope må bruges til databaseinitialisering; den er ikke den databasekontekst, som senere HTTP-requests bruger.

## IP-05: Læse- og skriveendpoints

**Leverance:** Endpoints til at oprette, hente én, hente oversigt og opdatere en draft, med request-/response-typer der følger API-kontrakten.

**Kontrolleres ved:**

- Create returnerer `201 Created` og den nye request.
- Read returnerer request eller `404 Not Found`.
- List returnerer nyeste requests først med stabil sekundær sortering.
- Update ændrer kun drafts og returnerer korrekt fejl ved en låst request.

**Vigtige aftaler:** Endpointet oversætter HTTP og JSON, men kalder domænemodellen for statusregler. Systemstyrede felter kommer ikke fra klientens request-body.

## IP-06: Workflow-endpoints

**Leverance:** Endpoints til send til review, approve og reject.

**Kontrolleres ved:**

- Et komplet draft kan blive `Submitted`.
- Approve og reject kan kun udføres fra `Submitted`.
- Reject uden begrundelse giver en fejl og ændrer ikke data.
- Alle svar og fejl følger API-kontrakten.

## IP-07: Workflowtests

**Leverance:** Automatiserede tests for `AT-01`–`AT-08`.

**Kontrolleres ved:** Testene starter appen i en test-host, bruger en isoleret database og kalder API’et som en HTTP-klient. Testene kan køres i vilkårlig rækkefølge uden at påvirke hinanden.

## IP-08: TypeScript-brugerflade

**Leverance:** Oversigt, oprettelse, detaljevisning, draft-redigering og reviewhandlinger i browseren.

**Kontrolleres ved:** Brugerfladen følger tabellen “Brugerfladens beslutninger” i brugerrejse-dokumentet og viser fejl fra API’et på en forståelig måde.

**Vigtige aftaler:** TypeScript-kildekode og genererede browserfiler har forskellige roller. Kildekoden redigeres; den genererede JavaScript-fil opdateres af build-trinnet og udgives til browseren.

## IP-09: Manuel test og fejlretning

**Leverance:** En gennemført testkørsel af `MT-01`–`MT-07` samt registrerede resultater.

**Kontrolleres ved:** Hver testcase er markeret bestået eller fejlet. Fejl kobles til et krav, en regel eller en UI-beslutning og bliver rettet med relevante automatiserede tests, hvor det er muligt.

## IP-10: Afsluttende review

**Leverance:** En opdateret sporbarhedstabel, en læsbarhedsgennemgang og GitHub-klar projektdokumentation.

**Kontrolleres ved:**

- Hvert funktionelt krav og hver vigtig regel kan spores til kode og mindst én test.
- API-kontrakt, TypeScript-typer og endpoint-adfærd stemmer overens.
- Navne følger ordlisten konsekvent.
- Ubrugte filer, lokale databaser og build-output er ikke med i Git.
- README forklarer formål, opstart, tekniske beslutninger og bevidste fravalg kort.

## Håndtering af ændringer under implementering

En observation under implementering kan vise, at planen er ufuldstændig. Den rigtige reaktion afhænger af typen:

| Observation | Første sted der opdateres |
|---|---|
| Ny eller ændret forretningsregel | Krav og forretningsregler; eventuelt brugerflow og statusmodel |
| Nyt eller ændret JSON-felt/endpointsvar | API-kontrakt; eventuelt domænemodel |
| Nyt tværgående teknisk mønster | Arkitektur og tekniske beslutninger |
| Ny navngivningsaftale | Domæneord og navngivning; kodekonventioner |
| Fundet fejl i eksisterende adfærd | Testcase, derefter relevant krav eller beslutning, hvis forståelsen også ændres |

Det betyder ikke, at hvert lokalt kodevalg kræver dokumentændring. Dokumentationen opdateres, når et valg ændrer fælles forståelse, kontrakt, regel, arkitektur eller reviewgrundlag.
