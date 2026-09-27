type ChangeRequestStatus = "Draft" | "Submitted" | "Approved" | "Rejected";

type ChangeRequest = {
    id: string;
    title: string;
    description: string;
    technicalImpact: string;
    status: ChangeRequestStatus;
    reviewComment: string | null;
    createdAt: string;
    reviewedAt: string | null;
};

type ChangeRequestDetails = {
    title: string;
    description: string;
    technicalImpact: string;
};

// API client

async function loadChangeRequests(): Promise<ChangeRequest[]> {
    const response = await fetch("/change-requests");

    if (!response.ok) {
        throw new Error(`Could not load change requests: ${response.status}`);
    }

    return response.json() as Promise<ChangeRequest[]>;
}

async function loadChangeRequest(id: string): Promise<ChangeRequest> {
    const response = await fetch(`/change-requests/${id}`);

    if (!response.ok) {
        throw new Error(`Could not load the change request: ${response.status}`);
    }

    return response.json() as Promise<ChangeRequest>;
}

async function createChangeRequest(details: ChangeRequestDetails): Promise<void> {
    const response = await fetch("/change-requests", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(details)
    });

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not create the change request."));
    }
}

async function updateChangeRequest(id: string, details: ChangeRequestDetails): Promise<void> {
    const response = await fetch(`/change-requests/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(details)
    });

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not update the change request."));
    }
}

async function sendChangeRequestToReview(id: string): Promise<void> {
    const response = await fetch(`/change-requests/${id}/submit-for-review`, {
        method: "POST"
    });

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not send the change request to review."));
    }
}

async function reviewChangeRequest(
    id: string,
    decision: "approve" | "reject",
    reviewComment: string): Promise<void> {
    const response = await fetch(`/change-requests/${id}/${decision}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ reviewComment })
    });

    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not review the change request."));
    }
}

async function getErrorMessage(response: Response, fallback: string): Promise<string> {
    try {
        const body = await response.json() as { error?: unknown };
        return typeof body.error === "string" ? body.error : fallback;
    } catch {
        return fallback;
    }
}

// Overview and draft creation

function renderPage(app: HTMLElement, changeRequests: ChangeRequest[]): void {
    const heading = document.createElement("h1");
    heading.textContent = "Change Request Approval";
    app.append(heading);

    const introduction = document.createElement("p");
    introduction.textContent = "Technical changes awaiting review.";
    app.append(introduction);

    app.append(renderCreateForm());
    renderChangeRequests(app, changeRequests);
}

function renderCreateForm(): HTMLFormElement {
    const form = document.createElement("form");

    const heading = document.createElement("h2");
    heading.textContent = "Create draft";
    form.append(heading);

    form.append(createTextField("Title", "title", false));
    form.append(createTextField("Description", "description", true));
    form.append(createTextField("Technical impact", "technicalImpact", true));

    const saveButton = document.createElement("button");
    saveButton.type = "submit";
    saveButton.textContent = "Save draft";
    form.append(saveButton);

    const errorMessage = document.createElement("p");
    errorMessage.setAttribute("role", "alert");
    form.append(errorMessage);

    form.addEventListener("submit", event => {
        void handleCreateSubmit(event, errorMessage, saveButton);
    });

    return form;
}

function createTextField(
    labelText: string,
    name: string,
    multiline: boolean,
    value = ""): HTMLParagraphElement {
    const field = document.createElement("p");
    const label = document.createElement("label");
    label.htmlFor = name;
    label.textContent = labelText;
    field.append(label);

    const input = multiline
        ? document.createElement("textarea")
        : document.createElement("input");

    input.id = name;
    input.name = name;
    input.value = value;
    field.append(input);

    return field;
}

// Create-draft event handler

async function handleCreateSubmit(
    event: SubmitEvent,
    errorMessage: HTMLParagraphElement,
    saveButton: HTMLButtonElement): Promise<void> {
    event.preventDefault();

    const form = event.currentTarget;
    if (!(form instanceof HTMLFormElement)) {
        return;
    }

    saveButton.disabled = true;
    errorMessage.textContent = "";

    try {
        await createChangeRequest(readChangeRequestDetails(form));

        await start();
    } catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not create the change request.";
    } finally {
        saveButton.disabled = false;
    }
}

function getTextValue(formData: FormData, name: string): string {
    const value = formData.get(name);
    return typeof value === "string" ? value : "";
}

// Change request list and detail views

function renderChangeRequests(app: HTMLElement, changeRequests: ChangeRequest[]): void {
    const heading = document.createElement("h2");
    heading.textContent = "Change requests";
    app.append(heading);

    if (changeRequests.length === 0) {
        const emptyMessage = document.createElement("p");
        emptyMessage.textContent = "No change requests have been created yet.";
        app.append(emptyMessage);
        return;
    }

    const list = document.createElement("ul");

    for (const changeRequest of changeRequests) {
        const item = document.createElement("li");
        const createdAt = formatDate(changeRequest.createdAt);

        const description = document.createElement("span");
        description.textContent = `${changeRequest.title} — ${changeRequest.status} — ${createdAt} `;
        item.append(description);

        const viewButton = document.createElement("button");
        viewButton.type = "button";
        viewButton.textContent = "View";
        viewButton.addEventListener("click", () => {
            void showChangeRequest(changeRequest.id);
        });
        item.append(viewButton);

        list.append(item);
    }

    app.append(list);
}

async function showChangeRequest(id: string): Promise<void> {
    const app = getApplicationRoot();
    app.replaceChildren();

    try {
        const changeRequest = await loadChangeRequest(id);
        renderChangeRequestDetail(app, changeRequest);
    } catch (error) {
        renderLoadError(app, error);
    }
}

function renderChangeRequestDetail(app: HTMLElement, changeRequest: ChangeRequest): void {
    const backButton = document.createElement("button");
    backButton.type = "button";
    backButton.textContent = "Back to overview";
    backButton.addEventListener("click", () => {
        void start();
    });
    app.append(backButton);

    const heading = document.createElement("h1");
    heading.textContent = changeRequest.title || "Untitled draft";
    app.append(heading);

    const details = document.createElement("dl");
    appendDetail(details, "Status", changeRequest.status);
    appendDetail(details, "Description", changeRequest.description);
    appendDetail(details, "Technical impact", changeRequest.technicalImpact);
    appendDetail(details, "Created", formatDate(changeRequest.createdAt));

    if (changeRequest.reviewComment !== null) {
        appendDetail(details, "Review comment", changeRequest.reviewComment);
    }

    if (changeRequest.reviewedAt !== null) {
        appendDetail(details, "Reviewed", formatDate(changeRequest.reviewedAt));
    }

    app.append(details);

    if (changeRequest.status === "Draft") {
        app.append(renderDraftEditForm(changeRequest));
    }

    if (changeRequest.status === "Submitted") {
        app.append(renderReviewForm(changeRequest));
    }
}

function renderDraftEditForm(changeRequest: ChangeRequest): HTMLFormElement {
    const form = document.createElement("form");

    const heading = document.createElement("h2");
    heading.textContent = "Edit draft";
    form.append(heading);

    form.append(createTextField("Title", "title", false, changeRequest.title));
    form.append(createTextField("Description", "description", true, changeRequest.description));
    form.append(createTextField(
        "Technical impact",
        "technicalImpact",
        true,
        changeRequest.technicalImpact));

    const saveButton = document.createElement("button");
    saveButton.type = "submit";
    saveButton.textContent = "Save changes";
    form.append(saveButton);

    const sendToReviewButton = document.createElement("button");
    sendToReviewButton.type = "button";
    sendToReviewButton.textContent = "Send to review";
    form.append(sendToReviewButton);

    const errorMessage = document.createElement("p");
    errorMessage.setAttribute("role", "alert");
    form.append(errorMessage);

    form.addEventListener("submit", event => {
        void handleDraftUpdateSubmit(event, changeRequest.id, errorMessage, saveButton);
    });

    sendToReviewButton.addEventListener("click", () => {
        void handleSendToReview(
            form,
            changeRequest.id,
            errorMessage,
            saveButton,
            sendToReviewButton);
    });

    return form;
}

function renderReviewForm(changeRequest: ChangeRequest): HTMLFormElement {
    const form = document.createElement("form");

    const heading = document.createElement("h2");
    heading.textContent = "Review request";
    form.append(heading);

    form.append(createTextField("Review comment", "reviewComment", true));

    const approveButton = document.createElement("button");
    approveButton.type = "button";
    approveButton.textContent = "Approve";
    approveButton.className = "approve-action";
    form.append(approveButton);

    const rejectButton = document.createElement("button");
    rejectButton.type = "button";
    rejectButton.textContent = "Reject";
    rejectButton.className = "reject-action";
    form.append(rejectButton);

    const errorMessage = document.createElement("p");
    errorMessage.setAttribute("role", "alert");
    form.append(errorMessage);

    approveButton.addEventListener("click", () => {
        void handleReviewSubmit(
            form,
            changeRequest.id,
            "approve",
            errorMessage,
            approveButton,
            rejectButton);
    });

    rejectButton.addEventListener("click", () => {
        void handleReviewSubmit(
            form,
            changeRequest.id,
            "reject",
            errorMessage,
            approveButton,
            rejectButton);
    });

    return form;
}

// Draft and review event handlers

async function handleDraftUpdateSubmit(
    event: SubmitEvent,
    id: string,
    errorMessage: HTMLParagraphElement,
    saveButton: HTMLButtonElement): Promise<void> {
    event.preventDefault();

    const form = event.currentTarget;
    if (!(form instanceof HTMLFormElement)) {
        return;
    }

    saveButton.disabled = true;
    errorMessage.textContent = "";

    try {
        await updateChangeRequest(id, readChangeRequestDetails(form));

        await showChangeRequest(id);
    } catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not update the change request.";
    } finally {
        saveButton.disabled = false;
    }
}

async function handleSendToReview(
    form: HTMLFormElement,
    id: string,
    errorMessage: HTMLParagraphElement,
    saveButton: HTMLButtonElement,
    sendToReviewButton: HTMLButtonElement): Promise<void> {
    saveButton.disabled = true;
    sendToReviewButton.disabled = true;
    errorMessage.textContent = "";

    try {
        // Keep "Send to review" as one UI action while submitting persisted data.
        await updateChangeRequest(id, readChangeRequestDetails(form));
        await sendChangeRequestToReview(id);
        await showChangeRequest(id);
    } catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not send the change request to review.";
    } finally {
        saveButton.disabled = false;
        sendToReviewButton.disabled = false;
    }
}

async function handleReviewSubmit(
    form: HTMLFormElement,
    id: string,
    decision: "approve" | "reject",
    errorMessage: HTMLParagraphElement,
    approveButton: HTMLButtonElement,
    rejectButton: HTMLButtonElement): Promise<void> {
    approveButton.disabled = true;
    rejectButton.disabled = true;
    errorMessage.textContent = "";

    try {
        const formData = new FormData(form);
        await reviewChangeRequest(id, decision, getTextValue(formData, "reviewComment"));
        await showChangeRequest(id);
    } catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not review the change request.";
    } finally {
        approveButton.disabled = false;
        rejectButton.disabled = false;
    }
}

// Form and display helpers

function readChangeRequestDetails(form: HTMLFormElement): ChangeRequestDetails {
    const formData = new FormData(form);

    return {
        title: getTextValue(formData, "title"),
        description: getTextValue(formData, "description"),
        technicalImpact: getTextValue(formData, "technicalImpact")
    };
}

function appendDetail(details: HTMLDListElement, labelText: string, value: string): void {
    const label = document.createElement("dt");
    label.textContent = labelText;
    details.append(label);

    const valueElement = document.createElement("dd");
    valueElement.textContent = value || "—";
    details.append(valueElement);
}

function formatDate(value: string): string {
    return new Intl.DateTimeFormat("en-GB", {
        dateStyle: "medium",
        timeStyle: "short"
    }).format(new Date(value));
}

function renderLoadError(app: HTMLElement, error: unknown): void {
    const message = document.createElement("p");
    message.textContent = error instanceof Error
        ? error.message
        : "Could not load change requests.";
    app.append(message);
}

// Application entry point

async function start(): Promise<void> {
    const app = getApplicationRoot();
    app.replaceChildren();

    try {
        const changeRequests = await loadChangeRequests();
        renderPage(app, changeRequests);
    } catch (error) {
        renderLoadError(app, error);
    }
}

function getApplicationRoot(): HTMLElement {
    const app = document.querySelector<HTMLElement>("#app");

    if (app === null) {
        throw new Error("The application root element was not found.");
    }

    return app;
}

void start();
