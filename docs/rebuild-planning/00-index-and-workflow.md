# Dokumentoversigt og arbejdsflow

## Formål

Denne mappe beskriver, hvordan **Change Request Approval** kunne være planlagt, afklaret, implementeret og reviewet fra starten.

Den nuværende app er referenceimplementeringen. Dokumenterne i denne mappe erstatter ikke de eksisterende arbejdsnoter; de er en ny, separat og mere systematisk planlægningspakke, som senere kan sammenlignes med koden.

Målet er ikke at forudsige hver linje kode. Målet er, at en ny udvikler, reviewer eller arkitekt hurtigt kan forstå:

- hvad systemet skal gøre
- hvilke begreber og regler der gælder
- hvilke beslutninger teamet har truffet, og hvorfor
- hvilke dele af koden der implementerer hvert krav
- hvilke automatiserede og manuelle tests der viser, at kravet er opfyldt

## Arbejdsflow

```text
Projektbeskrivelse og scope
        ↓
Fælles domænesprog
        ↓
Brugerrejser, regler og flows
        ↓
Statusmodel, data og API-kontrakt
        ↓
Arkitektur og tekniske beslutninger
        ↓
Teststrategi og planlagte testcases
        ↓
Små implementeringsopgaver
        ↓
Kode-review, manuel test og sporbarhedskontrol
        ↓
Klar til GitHub
```

Et senere trin må gerne afdække, at et tidligere trin mangler noget. Så går teamet tilbage og afklarer eller opdaterer beslutningen, før man fortsætter. Flowet er altså ikke en énvejsproces.

## Dokumentoversigt

| Fil | Spørgsmål den besvarer | Primært anvendt ved | Primær målgruppe |
|---|---|---|---|
| `01-project-brief.md` | Hvilket problem løser vi, for hvem, og hvad bygger vi bevidst ikke? | Afklaring og change request-review | Alle involverede |
| `02-domain-glossary-and-naming.md` | Hvad betyder de centrale domæneord, og hvilke navne bruger vi konsekvent? | Afklaring, implementering og review | Domænefolk, udviklere, testere |
| `03-requirements-and-business-rules.md` | Hvilken adfærd og hvilke regler skal systemet have? | Afklaring, testplan og review | Product Owner, udviklere, testere |
| `04-user-journeys-and-business-flow.md` | Hvilke brugerforløb og beslutningspunkter findes? | Afklaring, testplan og review | Product Owner, testere, udviklere |
| `05-state-and-domain-model.md` | Hvilke statusser, overgange og data findes? | Design, implementering og review | Udviklere, reviewere |
| `06-architecture-and-technical-decisions.md` | Hvilke dele består systemet af, og hvorfor er de valgt? | Teknisk afklaring og review | Udviklere, arkitekt, reviewere |
| `07-api-contract.md` | Hvilke HTTP-handlinger, input, svar og fejl kan klienten bruge? | Design, implementering, test og review | Frontend- og backendudviklere, testere |
| `08-test-strategy-and-manual-test-cases.md` | Hvad testes hvor, og hvordan afprøves brugerrejsen manuelt? | Testplan og testudførelse | Udviklere, testere, reviewere |
| `09-implementation-plan.md` | Hvilke små opgaver implementeres i hvilken rækkefølge? | Implementeringsplanlægning | Udviklere |
| `10-traceability-and-review.md` | Hvor er hvert krav implementeret og testet, og hvad mangler? | Implementeringsreview og afsluttende kvalitetssikring | Reviewere, udviklere |
| `11-code-structure-and-team-conventions.md` | Hvordan organiserer, navngiver og kommenterer vi kode? | Implementering og review | Udviklere, reviewere |

Dokumenterne kan oprettes og gennemgås ét ad gangen. Tabellen er et indeks, ikke en ordre om at skrive alt på én gang.

`08-test-strategy-and-manual-test-cases.md` beskriver de planlagte testscenarier og deres procedurer. `10-traceability-and-review.md` registrerer, hvor de er implementeret, og resultatet af konkrete testkørsler. De to dokumenter supplerer hinanden og må ikke forveksles.

## Beslutningsniveauer

Ikke alle valg skal behandles ens.

| Niveau | Eksempler | Hvordan håndteres det? |
|---|---|---|
| Domæne- og produktbeslutning | Hvad betyder "submit"? Skal reject kræve kommentar? | Afklares med domænefolk/Product Owner; skrives som regler og flows |
| Tværgående teknisk beslutning | Én app eller microservices? SQLite eller serverdatabase? API-kontrakt? | Drøftes af teamet; begrundelse skrives ned |
| Fælles implementeringsaftale | Navngivning, filstruktur, testnavne, fejlformat | Aftales af udviklerteamet; skrives som konvention |
| Lokal implementeringsdetalje | Lokal løkke, privat hjælpefunktion, kortvarig variabel | Vælges af udvikleren inden for konventionerne; vurderes ved review |

En udvikler skal ikke alene opfinde et nyt domænebegreb, en API-strategi eller et tværgående mønster. Lokale valg skal derimod ikke kræve et teammøde, når de er tydelige og følger de fælles aftaler.

## Sporbarhed

Hvert væsentligt krav får et stabilt id.

```text
BR-03  Forretningsregel: en draft skal være komplet før submit
AT-02  Acceptance-test: klient kan submitte en komplet draft
MT-01  Manuel test: bruger forsøger at submitte en ufuldstændig draft
TD-04  Teknisk beslutning: domæneregler ejes af ChangeRequest
```

Den senere sporbarhedstabel kobler id’erne til konkrete steder:

| Id | Kode | Automatiseret test | Manuel test | Status |
|---|---|---|---|---|
| BR-03 | `ChangeRequest.SubmitForReview()` | `ChangeRequestTests` og workflow-test | MT-01 | Eksempel |

Automatiserede tests er den stærkeste, eksekverbare dokumentation af reglerne. Diagrammer, kontrakter og tekst giver overblik, kontekst og fælles sprog; tests viser, at den implementerede kode faktisk opfører sig som aftalt.

## Review-flow for en funktion

Før en funktion betragtes som færdig, kontrolleres:

1. Findes dens brugerbehov og regel i dokumentationen?
2. Er domæneord og navne i kode konsistente med ordlisten?
3. Kan man finde den ansvarlige klasse/metode fra regel- eller flow-dokumentet?
4. Findes relevante unit- og acceptance-tests?
5. Findes relevant manuel test, når browseradfærd eller brugeroplevelse er vigtig?
6. Er tekniske valg og bevidste fravalg synlige, hvis de har konsekvenser for vedligeholdelse eller review?
7. Er dokumentets sporbarhedstabel opdateret?
