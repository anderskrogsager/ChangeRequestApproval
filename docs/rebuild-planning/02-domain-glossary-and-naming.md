# Domæneordliste og navngivning

## Formål

Dette dokument fastlægger de ord, som bruges konsekvent om problemområdet. Samme begreb skal have samme navn i samtaler, krav, diagrammer, API-kontrakter, testnavne og kode, medmindre der findes en reel og dokumenteret forskel.

Målet er at undgå, at én del af systemet taler om en "ticket", en anden om en "task" og en tredje om en "request", når de i virkeligheden mener det samme.

## Centrale domænebegreber

| Foretrukket begreb | Betydning i systemet | Må ikke forveksles med |
|---|---|---|
| Change request | Forslag til en teknisk ændring, der skal vurderes før implementering. | Pull request, opgave, ticket eller den faktiske kodeændring. |
| Draft | En oprettet change request, som stadig må være ufuldstændig og redigeres. | Rejected request eller en gemt, færdig løsning. |
| Send to review | Handlingen der sender en komplet draft til review. | Almindelig gemning, implementering, deployering eller merge. |
| Submitted | Status efter en vellykket handling “send to review” og før en reviewer har truffet beslutning. | Approved eller in progress. |
| Review | Vurderingen af en submitted change request. Udfaldet er approve eller reject. | Kode-review af en pull request. |
| Approve | At acceptere en submitted change request som klar til implementering. | At merge kode eller markere en opgave som afsluttet. |
| Reject | At afvise en submitted change request. En begrundelse er obligatorisk. | At slette requesten. |
| Technical impact | Beskrivelse af berørte systemer, afhængigheder, risici og deploy-overvejelser. | En fuld implementeringsplan eller et simpelt systemnavn. |
| Review comment | Kommentar fra reviewet. Valgfri ved approve, obligatorisk ved reject. | Description, som er requesters beskrivelse af selve ændringen. |
| Final request | En approved eller rejected change request. Den kan ikke længere ændres eller reviewes. | Submitted request, som stadig afventer beslutning. |

## Roller

| Rolle | Betydning | Begrænsning i første version |
|---|---|---|
| Requester | Den person, der opretter, redigerer og submitter en change request. | Systemet gemmer ikke personens identitet. |
| Reviewer | Den person, der approver eller rejecter en submitted change request. | Systemet kontrollerer ikke, hvem der udfører handlingen. |

Rollerne forklarer workflowet. De er ikke implementerede brugerroller eller autorisationsregler.

## Statusord og overgangsord

```text
Draft → Submitted → Approved
                  → Rejected
```

| Status | Betydning | Tilladte handlinger |
|---|---|---|
| `Draft` | Oprettet og redigerbar; kan være ufuldstændig. | Update details, send to review |
| `Submitted` | Komplet og afventer review. | Approve, reject |
| `Approved` | Reviewet er afsluttet med accept. | Ingen |
| `Rejected` | Reviewet er afsluttet med afvisning og kommentar. | Ingen |

En **statusovergang** er en handling, der ændrer status. `Update details` er ikke en statusovergang; den ændrer data på en request, som stadig er `Draft`.

## Navne i kode og API

| Domænebegreb | C# | TypeScript/JSON | Bemærkning |
|---|---|---|---|
| Change request | `ChangeRequest` | `ChangeRequest` som TypeScript-type | Klasse/type navngives efter domænebegrebet. |
| Status | `ChangeRequestStatus` | `ChangeRequestStatus` | Enum-værdierne bruger de samme statusord. |
| Opret-input | `CreateChangeRequestRequest` | JSON-body for `POST /change-requests` | API-input navngives efter handlingen. |
| Opdater-input | `UpdateChangeRequestRequest` | JSON-body for `PUT /change-requests/{id}` | Samme felter nu, men separat kontrakt og ansvar. |
| Approve-input | `ApproveChangeRequestRequest` | JSON-body for approve | Indeholder valgfri `reviewComment`. |
| Reject-input | `RejectChangeRequestRequest` | JSON-body for reject | Indeholder `reviewComment`, som domænet kræver ved reject. |
| Technical impact | `TechnicalImpact` | `technicalImpact` | Samme ord på tværs af grænser. |
| Review comment | `ReviewComment` | `reviewComment` | Samme ord på tværs af grænser. |

Forskellen mellem `PascalCase` i C# og `camelCase` i JSON/TypeScript er en sproglig navnekonvention, ikke en forskel i domænebetydning.

## Navngivningsaftaler

### Domæneord først

Et navn skal indeholde det relevante domæneord, når typen eller den lokale sammenhæng ikke allerede gør betydningen entydig.

```csharp
ChangeRequest changeRequest
string reviewComment
ChangeRequestStatus status
```

Undgå upræcise erstatninger som `item`, `data`, `model`, `payload` eller `result`, når de skjuler et kendt domænebegreb.

### Tekniske navne må beskrive deres tekniske rolle

Et teknisk objekt må have et teknisk navn, når det ikke repræsenterer et domænebegreb.

```csharp
ChangeRequestDbContext database
HttpClient client
HttpResponseMessage response
```

I en lille og tydeligt afgrænset funktion kan `request`, `response` eller `id` være passende, hvis typen, endpointet eller den umiddelbare kontekst gør det klart, hvilken request, hvilket svar eller hvilket id der menes. Hvis flere slags findes samtidig, skal navnet præciseres, fx `createRequest`, `approveRequest` eller `changeRequestId`.

### Metodenavne beskriver handlingen

Metoder, der ændrer en change request, bruger de aftalte handlinger:

```csharp
UpdateDetails(...)
SubmitForReview()
Approve(...)
Reject(...)
```

Et nyt verb må ikke indføres som synonym uden en grund. Eksempelvis ville `Accept()` og `Approve()` skabe to ord for samme handling og bør undgås.

### Testnavne beskriver regel og udfald

Testnavne skal gøre læseren i stand til at forstå den testede regel uden at læse selve testen.

```text
User_can_approve_a_submitted_change_request
User_cannot_reject_a_submitted_change_request_without_a_comment
```

## Når et nyt begreb opstår

Før et nyt domæneord eller et nyt tværgående navn bruges, afklares:

1. Er det faktisk et nyt begreb, eller et eksisterende begreb med et nyt navn?
2. Hvem kan bekræfte betydningen: domæneperson, Product Owner eller udviklerteam?
3. Hvilket foretrukket navn skal bruges i samtaler, dokumentation, API, tests og kode?
4. Findes et eksisterende ord, der nu skal præciseres eller udfases?

Hvis svaret ændrer systemets regler, data eller API, opdateres denne ordliste sammen med de relevante krav og tests.

## Næste dokument

`03-requirements-and-business-rules.md` omsætter begreberne til identificerbare krav og forretningsregler.
