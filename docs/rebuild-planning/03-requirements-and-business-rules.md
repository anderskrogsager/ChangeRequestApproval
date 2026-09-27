# Krav og forretningsregler

## Hvordan dokumentet bruges

Dette dokument beskriver den forventede adfærd som små, identificerbare krav. Senere dokumenter beskriver brugerrejser, HTTP-kontrakter og testcases; de må ikke ændre betydningen af reglerne her.

| Id-type | Betydning |
|---|---|
| `FR-xx` | Funktionelt krav: noget en bruger eller klient skal kunne gøre eller se. |
| `BR-xx` | Forretningsregel: en regel der begrænser eller ændrer gyldig adfærd. |

## Funktionelle krav

| Id | Krav | Observerbart resultat |
|---|---|---|
| FR-01 | En bruger kan oprette en change request. | Der oprettes en request med status `Draft`. |
| FR-02 | En bruger kan se en oversigt over change requests. | Oversigten viser alle eksisterende requests, nyeste først. |
| FR-03 | En bruger kan åbne én konkret change request. | Brugeren kan se dens detaljer, status og eventuelt reviewresultat. |
| FR-04 | En bruger kan redigere detaljer på en draft. | De gemte draft-felter opdateres, mens status forbliver `Draft`. |
| FR-05 | En bruger kan sende en komplet draft til review. | Status ændres fra `Draft` til `Submitted`. |
| FR-06 | En reviewer kan approve en submitted request. | Status ændres til `Approved`; reviewtidspunkt og eventuel kommentar vises. |
| FR-07 | En reviewer kan rejecte en submitted request. | Status ændres til `Rejected`; reviewtidspunkt og begrundelse vises. |

## Forretningsregler

| Id | Regel | Forudsætning | Resultat ved gyldig handling | Resultat ved ugyldig handling |
|---|---|---|---|---|
| BR-01 | En ny request starter som draft. | Opret change request. | Systemet tildeler status `Draft`. | Ikke relevant. |
| BR-02 | Systemet tildeler identitet og oprettelsestidspunkt. | Opret change request. | Requesten har et unikt id og `CreatedAt` i UTC. | Ikke relevant. |
| BR-03 | Kun en draft må redigeres. | Status er `Draft`. | Title, Description og Technical impact kan ændres. | Ved enhver anden status afvises handlingen; eksisterende data ændres ikke. |
| BR-04 | En draft må være ufuldstændig. | Status er `Draft`. | Title, Description og Technical impact må være tomme eller mangle. | Ikke relevant. |
| BR-05 | Systemet accepterer kun handlingen "send til review" fra en draft. | Status er `Draft`. | Indholdsvalidering udføres. | Ved enhver anden status afvises handlingen; status ændres ikke. |
| BR-06 | En draft skal være komplet, før den kan sendes til review. | Status er `Draft`. | Når Title, Description og Technical impact alle indeholder ikke-blank tekst, ændres status til `Submitted`. | Hvis mindst ét af felterne er tomt, `null` eller kun whitespace, afvises handlingen; status forbliver `Draft`. |
| BR-07 | Kun en submitted request må reviewes. | Status er `Submitted`. | Approve eller reject kan udføres. | Ved `Draft`, `Approved` eller `Rejected` afvises review; status ændres ikke. |
| BR-08 | Approve afslutter requesten. | Status er `Submitted`. | Status ændres til `Approved`; `ReviewedAt` sættes til UTC-tidspunktet; reviewkommentar kan være fraværende. | Se BR-07. |
| BR-09 | Reject afslutter requesten og kræver begrundelse. | Status er `Submitted`. | Status ændres til `Rejected`; `ReviewedAt` sættes til UTC-tidspunktet; Review comment gemmes. | Hvis kommentaren er `null`, tom eller kun whitespace, afvises reject; status og reviewtidspunkt ændres ikke. |
| BR-10 | Final requests er låste. | Status er `Approved` eller `Rejected`. | Ingen yderligere workflowhandling er tilladt. | Update, submit, approve og reject afvises; eksisterende data ændres ikke. |

## Felter, validering og status

### Indhold, som requesteren kan udfylde

| Felt | Må være tomt i draft? | Hvornår valideres det? | Krav ved validering |
|---|---|---|---|
| Title | Ja | Når requesten sendes til review | Skal indeholde ikke-blank tekst. |
| Description | Ja | Når requesten sendes til review | Skal indeholde ikke-blank tekst. |
| Technical impact | Ja | Når requesten sendes til review | Skal indeholde ikke-blank tekst. |

Det er valideringstidspunktet: ikke ved oprettelse eller almindelig redigering, men når brugeren forsøger at sende draften til review.

### Systemstyrede felter og workflowkontrol

| Felt | Hvordan sættes det? | Regel |
|---|---|---|
| Id | Systemet ved oprettelse. | Er altid til stede og ændres ikke. |
| CreatedAt | Systemet ved oprettelse. | Er altid til stede, er i UTC og ændres ikke. |
| Status | Systemet ved oprettelse og workflowhandlinger. | Sættes til `Draft` ved oprettelse og er aldrig tom. Handlingen "send til review" er kun gyldig, når status er præcist `Draft`; den ændrer derefter status til `Submitted`. Review er kun gyldigt ved `Submitted`; det ændrer derefter status til `Approved` eller `Rejected`. |
| Review comment | Revieweren ved approve eller reject. | Valgfri ved approve; ikke-blank tekst kræves ved reject. |
| ReviewedAt | Systemet ved approve eller reject. | Sættes ved gyldig reviewhandling og ændres ikke derefter. |

`Status` er dermed ikke et felt, der blot skal være "udfyldt ved submit". Det er workflowets styrende tilstand: den har altid en værdi, og den bestemmer, om handlingen overhovedet er gyldig.

### Hvad brugerfladen viser

Forretningsreglerne bestemmer, hvad der er gyldigt. De bestemmer ikke i sig selv præcis, hvilke formularfelter browseren viser.

Den planlagte brugerfladeregel er:

- Title, Description og Technical impact kan redigeres, når status er `Draft`.
- Review comment kan indtastes, når status er `Submitted`.
- Approved og Rejected vises kun som læsbare, endelige resultater.

Det forklarer, hvorfor det ikke helt er korrekt at sige, at browseren kun viser inputfelter ved `Draft`: en submitted request har også et inputfelt, men kun til reviewkommentaren.

## Grænsetilfælde

| Situation | Forventet resultat | Relevant regel |
|---|---|---|
| Request oprettes uden felter. | Gyldig, tom `Draft`. | BR-01, BR-04 |
| Draft har kun en titel. | Må gemmes, men må ikke submitte. | BR-04, BR-06 |
| Title består kun af mellemrum. | Må gemmes i draft, men gør submit ugyldig. | BR-04, BR-06 |
| Submitted request forsøges opdateret. | Afvises; de tidligere detaljer bevares. | BR-03 |
| Draft forsøges approvet. | Afvises; status forbliver `Draft`. | BR-07 |
| Submitted request rejectes uden kommentar. | Afvises; status forbliver `Submitted`. | BR-09 |
| Approved request forsøges submitted igen. | Afvises; status forbliver `Approved`. | BR-10 |

## Fejlprincip

Når en forretningsregel afviser en handling:

1. Systemet ændrer ikke requestens status eller andre beskyttede data som følge af den fejlede handling.
2. Klienten får en forståelig fejlbesked.
3. Frontend må gerne vise en hjælpsom fejl tidligere, men må ikke være eneste sted, reglen håndhæves.

Det konkrete HTTP-format og statuskoder beskrives i API-kontrakten.

## Navn for handlingen "send til review"

I denne planlægningspakke betyder **send til review** handlingen fra `Draft` til `Submitted`. Det er den fulde brugerrettede formulering, fordi ordet "submit" alene let kan misforstås som almindelig gemning.

Domænemetoden hedder `SubmitForReview()`, og endpointet er `/submit-for-review`. Begge navne angiver handlingens betydning og adskiller den fra at gemme en draft.

## Ikke krav i denne version

- En rejected eller approved request kan ikke genåbnes.
- En request kan ikke slettes.
- Systemet gemmer ikke requester- eller revieweridentitet.
- Systemet validerer ikke indholdets faglige kvalitet; det kræver menneskelig vurdering i reviewet.

## Næste dokument

`04-user-journeys-and-business-flow.md` samler kravene til konkrete brugerforløb og beslutningspunkter.
