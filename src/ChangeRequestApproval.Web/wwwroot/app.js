"use strict";
// API client
async function loadChangeRequests() {
    const response = await fetch("/change-requests");
    if (!response.ok) {
        throw new Error(`Could not load change requests: ${response.status}`);
    }
    return response.json();
}
async function loadChangeRequest(id) {
    const response = await fetch(`/change-requests/${id}`);
    if (!response.ok) {
        throw new Error(`Could not load the change request: ${response.status}`);
    }
    return response.json();
}
async function createChangeRequest(details) {
    const response = await fetch("/change-requests", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(details)
    });
    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not create the change request."));
    }
}
async function updateChangeRequest(id, details) {
    const response = await fetch(`/change-requests/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(details)
    });
    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not update the change request."));
    }
}
async function sendChangeRequestToReview(id) {
    const response = await fetch(`/change-requests/${id}/submit-for-review`, {
        method: "POST"
    });
    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not send the change request to review."));
    }
}
async function reviewChangeRequest(id, decision, reviewComment) {
    const response = await fetch(`/change-requests/${id}/${decision}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ reviewComment })
    });
    if (!response.ok) {
        throw new Error(await getErrorMessage(response, "Could not review the change request."));
    }
}
async function getErrorMessage(response, fallback) {
    try {
        const body = await response.json();
        return typeof body.error === "string" ? body.error : fallback;
    }
    catch {
        return fallback;
    }
}
// Overview and draft creation
function renderPage(app, changeRequests) {
    const heading = document.createElement("h1");
    heading.textContent = "Change Request Approval";
    app.append(heading);
    const introduction = document.createElement("p");
    introduction.textContent = "Technical changes awaiting review.";
    app.append(introduction);
    app.append(renderCreateForm());
    renderChangeRequests(app, changeRequests);
}
function renderCreateForm() {
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
function createTextField(labelText, name, multiline, value = "") {
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
async function handleCreateSubmit(event, errorMessage, saveButton) {
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
    }
    catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not create the change request.";
    }
    finally {
        saveButton.disabled = false;
    }
}
function getTextValue(formData, name) {
    const value = formData.get(name);
    return typeof value === "string" ? value : "";
}
// Change request list and detail views
function renderChangeRequests(app, changeRequests) {
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
async function showChangeRequest(id) {
    const app = getApplicationRoot();
    app.replaceChildren();
    try {
        const changeRequest = await loadChangeRequest(id);
        renderChangeRequestDetail(app, changeRequest);
    }
    catch (error) {
        renderLoadError(app, error);
    }
}
function renderChangeRequestDetail(app, changeRequest) {
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
function renderDraftEditForm(changeRequest) {
    const form = document.createElement("form");
    const heading = document.createElement("h2");
    heading.textContent = "Edit draft";
    form.append(heading);
    form.append(createTextField("Title", "title", false, changeRequest.title));
    form.append(createTextField("Description", "description", true, changeRequest.description));
    form.append(createTextField("Technical impact", "technicalImpact", true, changeRequest.technicalImpact));
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
        void handleSendToReview(form, changeRequest.id, errorMessage, saveButton, sendToReviewButton);
    });
    return form;
}
function renderReviewForm(changeRequest) {
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
        void handleReviewSubmit(form, changeRequest.id, "approve", errorMessage, approveButton, rejectButton);
    });
    rejectButton.addEventListener("click", () => {
        void handleReviewSubmit(form, changeRequest.id, "reject", errorMessage, approveButton, rejectButton);
    });
    return form;
}
// Draft and review event handlers
async function handleDraftUpdateSubmit(event, id, errorMessage, saveButton) {
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
    }
    catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not update the change request.";
    }
    finally {
        saveButton.disabled = false;
    }
}
async function handleSendToReview(form, id, errorMessage, saveButton, sendToReviewButton) {
    saveButton.disabled = true;
    sendToReviewButton.disabled = true;
    errorMessage.textContent = "";
    try {
        // Keep "Send to review" as one UI action while submitting persisted data.
        await updateChangeRequest(id, readChangeRequestDetails(form));
        await sendChangeRequestToReview(id);
        await showChangeRequest(id);
    }
    catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not send the change request to review.";
    }
    finally {
        saveButton.disabled = false;
        sendToReviewButton.disabled = false;
    }
}
async function handleReviewSubmit(form, id, decision, errorMessage, approveButton, rejectButton) {
    approveButton.disabled = true;
    rejectButton.disabled = true;
    errorMessage.textContent = "";
    try {
        const formData = new FormData(form);
        await reviewChangeRequest(id, decision, getTextValue(formData, "reviewComment"));
        await showChangeRequest(id);
    }
    catch (error) {
        errorMessage.textContent = error instanceof Error
            ? error.message
            : "Could not review the change request.";
    }
    finally {
        approveButton.disabled = false;
        rejectButton.disabled = false;
    }
}
// Form and display helpers
function readChangeRequestDetails(form) {
    const formData = new FormData(form);
    return {
        title: getTextValue(formData, "title"),
        description: getTextValue(formData, "description"),
        technicalImpact: getTextValue(formData, "technicalImpact")
    };
}
function appendDetail(details, labelText, value) {
    const label = document.createElement("dt");
    label.textContent = labelText;
    details.append(label);
    const valueElement = document.createElement("dd");
    valueElement.textContent = value || "—";
    details.append(valueElement);
}
function formatDate(value) {
    return new Intl.DateTimeFormat("en-GB", {
        dateStyle: "medium",
        timeStyle: "short"
    }).format(new Date(value));
}
function renderLoadError(app, error) {
    const message = document.createElement("p");
    message.textContent = error instanceof Error
        ? error.message
        : "Could not load change requests.";
    app.append(message);
}
// Application entry point
async function start() {
    const app = getApplicationRoot();
    app.replaceChildren();
    try {
        const changeRequests = await loadChangeRequests();
        renderPage(app, changeRequests);
    }
    catch (error) {
        renderLoadError(app, error);
    }
}
function getApplicationRoot() {
    const app = document.querySelector("#app");
    if (app === null) {
        throw new Error("The application root element was not found.");
    }
    return app;
}
void start();
