// Fetch the CSRF token from the server.

document.addEventListener("DOMContentLoaded", () => {
    (async () => {
        // 1) Get CSRF token (await is now inside an async function)
        const r = await fetch("/antiforgery/token", { credentials: "include" });
        if (!r.ok) throw new Error("Failed to get antiforgery token: " + r.status);
        const j = await r.json();
        const csrfHeaderName = j.header || "X-CSRF-TOKEN";
        const csrfToken = j.token;

        // 2) Start SSE
        const es = new EventSource("/chat/stream");
        es.onmessage = (e) => {
            const m = JSON.parse(e.data);
            AppendReceivedMessage(m.AuthorName, m.Content, new Date(m.CreatedTime).toLocaleString());
        };
        es.onerror = (err) => console.error("SSE error", err);

        // 3) Send handler (uses the token)
        const sendBtn = document.getElementById("sendButton");
        const input = document.getElementById("messageInput");

        sendBtn.addEventListener("click", async (evt) => {
            evt.preventDefault();
            const payload = input.value;

            const resp = await fetch("/chat/send", {
                method: "POST",
                headers: {
                    "Accept": "application/json",
                    "Content-Type": "application/json",
                    [csrfHeaderName]: csrfToken
                },
                body: JSON.stringify(payload),
                credentials: "include"
            });

            if (!resp.ok) {
                console.error("POST /chat/send failed", resp.status, await resp.text());
                alert(`Send failed: ${resp.status}`);
                return;
            }
            input.value = "";
        });
    })().catch(err => console.error("Init error:", err));
});