# API-kontrakt

## Formål og afgrænsning

Dette dokument er kontrakten mellem en HTTP-klient og Change Request Approval. Den fastlægger de synlige HTTP-handlinger, JSON-formater, svar og fejl. Den beskriver ikke, hvordan serveren er implementeret internt.

API’et håndterer change requests. Alle feltnavne i JSON bruger `camelCase`.

## Fælles regler

| Emne | Aftale |
|---|---|
| Base path | `/change-requests` |
| Indholdsformat | Request- og response-body bruger JSON med `Content-Type: application/json`. |
| Id | En request identificeres med et GUID i URL’en. |
| Status | Returneres som tekst: `Draft`, `Submitted`, `Approved` eller `Rejected`. |
| Tidspunkter | Returneres som ISO 8601-tidspunkter i UTC. |
| Fejlformat | Forretningsfejl returnerer JSON på formen `{ "error": "forklarende tekst" }`. |
| Regelhåndhævelse | Serveren håndhæver reglerne, også når browseren normalt skjuler den ugyldige handling. |

## ChangeRequest i svar

Når API’et returnerer en change request, har svaret denne form:

```json
{
  "id": "edc863e5-a554-4a9c-9052-e8c9ff9aa910",
  "title": "Opdatér database-driver",
  "description": "Opdatér driveren til den understøttede version.",
  "technicalImpact": "Kræver regressionstest af dataadgang.",
  "status": "Submitted",
  "createdAt": "2026-09-22T09:30:00Z",
  "reviewComment": null,
  "reviewedAt": null
}
```

`reviewComment` og `reviewedAt` er `null`, før requesten er reviewed. Ved `Approved` normaliseres en manglende, blank eller kun-whitespace `reviewComment` til `null`; ved `Rejected` indeholder den en ikke-blank begrundelse.

## Endpoints

### Opret change request

```text
POST /change-requests
```

Request-body:

```json
{
  "title": "",
  "description": "",
  "technicalImpact": ""
}
```

Alle tre felter er valgfrie ved oprettelse. De kan være tomme eller mangle helt; serveren opretter i så fald en `Draft`.

| Svar | Betydning |
|---|---|
| `201 Created` | Requesten er oprettet. Body indeholder den nye change request, og `Location` peger på dens URL. |
| `400 Bad Request` | JSON-body kan ikke læses som request-data. |

Relaterede regler: FR-01, BR-01, BR-02, BR-04.

### Hent oversigt

```text
GET /change-requests
```

| Svar | Betydning |
|---|---|
| `200 OK` | Body er et JSON-array af change requests, sorteret nyeste først efter `createdAt`. Ved samme tidspunkt bruges `id` som stabil sekundær sortering. |

Relateret krav: FR-02.

### Hent én change request

```text
GET /change-requests/{id}
```

| Svar | Betydning |
|---|---|
| `200 OK` | Body indeholder change requesten. |
| `404 Not Found` | `{id}` er ikke et gyldigt GUID, eller der findes ingen request med dette id. |

Relateret krav: FR-03.

### Opdater en draft

```text
PUT /change-requests/{id}
```

Request-body:

```json
{
  "title": "Opdatér database-driver",
  "description": "Opdatér driveren til den understøttede version.",
  "technicalImpact": "Kræver regressionstest af dataadgang."
}
```

Alle tre felter erstattes af værdierne i body. De må fortsat være tomme i en `Draft`.

| Svar | Betydning |
|---|---|
| `200 OK` | Draften er opdateret. Body indeholder den opdaterede change request. |
| `400 Bad Request` | Body kan ikke læses, eller requesten er ikke en `Draft`. Fejl-body følger fælles fejlformat ved brud på forretningsregel. |
| `404 Not Found` | `{id}` er ikke et gyldigt GUID, eller der findes ingen request med dette id. |

Relaterede regler: FR-04, BR-03, BR-04.

### Send draft til review

```text
POST /change-requests/{id}/submit-for-review
```

Endpointet har ingen request-body. Det bruger den allerede gemte `Draft`.

### Klientens rækkefølge ved ændrede draftfelter

En klient, der lader brugeren redigere en draft og sende den til review i samme brugerhandling, skal udføre to kald i denne rækkefølge:

```text
1. PUT /change-requests/{id}
   med de tre aktuelle indholdsfelter.

2. POST /change-requests/{id}/submit-for-review
   uden request-body, først når PUT-kaldet er lykkedes.
```

Submit-endpointet læser kun den allerede gemte draft. Hvis PUT-kaldet lykkes, men submit-handlingen afvises, forbliver de nye indholdsfelter gemt på draften, mens status fortsat er `Draft`. Rækkefølgen er derfor en del af den adfærd, som enhver browser- eller API-klient skal kunne følge.

| Svar | Betydning |
|---|---|
| `200 OK` | Draften var komplet og har nu status `Submitted`. Body indeholder den opdaterede change request. |
| `400 Bad Request` | Requesten er ikke en `Draft`, eller mindst ét indholdsfelt er blankt. Fejl-body følger fælles fejlformat. |
| `404 Not Found` | `{id}` er ikke et gyldigt GUID, eller der findes ingen request med dette id. |

Relaterede regler: FR-05, BR-05, BR-06.

### Approve en submitted request

```text
POST /change-requests/{id}/approve
```

Request-body er valgfri. Hvis der sendes en body, har den formen:

```json
{
  "reviewComment": "Godkendt efter regressionstest."
}
```

`reviewComment` er valgfri ved approve.

| Svar | Betydning |
|---|---|
| `200 OK` | Requesten var `Submitted` og har nu status `Approved`. `reviewedAt` er sat. Body indeholder den opdaterede change request. |
| `400 Bad Request` | Body kan ikke læses, eller requesten er ikke `Submitted`. Fejl-body følger fælles fejlformat ved brud på forretningsregel. |
| `404 Not Found` | `{id}` er ikke et gyldigt GUID, eller der findes ingen request med dette id. |

Relaterede regler: FR-06, BR-07, BR-08.

### Reject en submitted request

```text
POST /change-requests/{id}/reject
```

Request-body:

```json
{
  "reviewComment": "Mangler vurdering af påvirkning på backup-jobbet."
}
```

`reviewComment` er påkrævet og skal indeholde ikke-blank tekst.

| Svar | Betydning |
|---|---|
| `200 OK` | Requesten var `Submitted`, har nu status `Rejected`, og reviewkommentar samt `reviewedAt` er gemt. Body indeholder den opdaterede change request. |
| `400 Bad Request` | Body kan ikke læses, requesten er ikke `Submitted`, eller reviewComment er blank. Fejl-body følger fælles fejlformat ved brud på forretningsregel. |
| `404 Not Found` | `{id}` er ikke et gyldigt GUID, eller der findes ingen request med dette id. |

Relaterede regler: FR-07, BR-07, BR-09.

## Kontraktens grænse

Kontrakten siger, hvad klienten kan sende og forvente. Den giver ikke klienten ret til at ændre systemstyrede felter som `id`, `status`, `createdAt` eller `reviewedAt`; de findes derfor ikke i create- eller update-body.

En klient må heller ikke slutte, at en skjult knap betyder, at en handling er tilladt. Endpointet kontrollerer status og data på serveren for hvert kald.
