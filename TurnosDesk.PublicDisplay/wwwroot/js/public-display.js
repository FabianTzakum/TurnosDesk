const config = window.TURNOSDESK_CONFIG ?? {};
const API_BASE_URL = config.apiBaseUrl ?? "https://localhost:7001";
const HUB_URL = `${API_BASE_URL}/hubs/queue`;

const connectionStatus = document.getElementById("connectionStatus");
const currentTicketFolio = document.getElementById("currentTicketFolio");
const currentTicketModule = document.getElementById("currentTicketModule");
const currentTicketService = document.getElementById("currentTicketService");
const ticketHistory = document.getElementById("ticketHistory");
const currentTime = document.getElementById("currentTime");

const tickets = [];

function setConnectionStatus(status) {
    const text = connectionStatus.querySelector(".connection-status__text");

    connectionStatus.classList.remove("is-online", "is-offline");

    if (status === "online") {
        connectionStatus.classList.add("is-online");
        text.textContent = "Conectado";
        return;
    }

    if (status === "offline") {
        connectionStatus.classList.add("is-offline");
        text.textContent = "Sin conexión";
        return;
    }

    text.textContent = "Conectando";
}

function getStatusLabel(status) {
    const labels = {
        Pending: "Pendiente",
        Called: "Llamado",
        InService: "En atención",
        Completed: "Finalizado",
        Cancelled: "Cancelado",
        NoShow: "No presentado"
    };

    return labels[status] ?? status;
}

function normalizeTicket(ticket) {
    return {
        id: ticket.id ?? ticket.ticketId ?? 0,
        folio: ticket.folio ?? "---",
        status: ticket.status ?? "Called",
        serviceTypeName: ticket.serviceTypeName ?? "Servicio no disponible",
        serviceModuleName: ticket.serviceModuleName ?? "Módulo no asignado",
        calledAt: ticket.calledAt ?? ticket.occurredAt ?? null
    };
}

function updateCurrentTicket(ticket) {
    const normalizedTicket = normalizeTicket(ticket);

    currentTicketFolio.textContent = normalizedTicket.folio;
    currentTicketModule.textContent = normalizedTicket.serviceModuleName
        ? `Acuda a ${normalizedTicket.serviceModuleName}`
        : "Módulo no asignado";
    currentTicketService.textContent = normalizedTicket.serviceTypeName;
}

function addTicketToHistory(ticket) {
    const normalizedTicket = normalizeTicket(ticket);

    tickets.unshift(normalizedTicket);

    const uniqueTickets = [];
    const seen = new Set();

    for (const item of tickets) {
        const identity = item.id > 0 ? item.id : item.folio;

        if (seen.has(identity)) {
            continue;
        }

        seen.add(identity);
        uniqueTickets.push(item);
    }

    tickets.length = 0;
    tickets.push(...uniqueTickets.slice(0, 8));

    renderHistory();
}

function renderHistory() {
    if (tickets.length === 0) {
        ticketHistory.innerHTML = '<li class="ticket-list__empty">Aún no hay turnos llamados en esta sesión.</li>';
        return;
    }

    ticketHistory.innerHTML = tickets
        .map(ticket => `
            <li class="ticket-list__item">
                <strong class="ticket-list__folio">${ticket.folio}</strong>
                <div class="ticket-list__meta">
                    <p class="ticket-list__module">${ticket.serviceModuleName}</p>
                    <p class="ticket-list__service">${ticket.serviceTypeName}</p>
                </div>
                <span class="ticket-list__status">${getStatusLabel(ticket.status)}</span>
            </li>
        `)
        .join("");
}

function handleRealtimeTicket(ticket) {
    updateCurrentTicket(ticket);
    addTicketToHistory(ticket);
}

async function loadInitialDashboardData() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/dashboard/summary`);

        if (!response.ok) {
            return;
        }

        const payload = await response.json();
        const latestCalledTickets = payload?.data?.latestCalledTickets ?? [];

        latestCalledTickets.forEach(ticket => {
            addTicketToHistory({
                id: ticket.ticketId,
                folio: ticket.folio,
                status: ticket.status,
                serviceTypeName: ticket.serviceTypeName,
                serviceModuleName: ticket.serviceModuleName,
                calledAt: ticket.calledAt
            });
        });

        if (latestCalledTickets.length > 0) {
            updateCurrentTicket({
                id: latestCalledTickets[0].ticketId,
                folio: latestCalledTickets[0].folio,
                status: latestCalledTickets[0].status,
                serviceTypeName: latestCalledTickets[0].serviceTypeName,
                serviceModuleName: latestCalledTickets[0].serviceModuleName,
                calledAt: latestCalledTickets[0].calledAt
            });
        }
    } catch (error) {
        console.warn("No se pudo cargar el resumen inicial de turnos.", error);
    }
}

function updateClock() {
    const now = new Date();

    currentTime.textContent = now.toLocaleTimeString("es-MX", {
        hour: "2-digit",
        minute: "2-digit"
    });
}

async function startConnection() {
    if (!window.signalR) {
        setConnectionStatus("offline");
        console.error("SignalR no está disponible. Revisa la carga del CDN.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .build();

    connection.on("TicketCalled", ticket => {
        handleRealtimeTicket(ticket);
    });

    connection.on("TicketStarted", ticket => {
        addTicketToHistory(ticket);
    });

    connection.on("TicketCompleted", ticket => {
        addTicketToHistory(ticket);
    });

    connection.on("TicketNoShow", ticket => {
        addTicketToHistory(ticket);
    });

    connection.on("TicketCancelled", ticket => {
        addTicketToHistory(ticket);
    });

    connection.onreconnecting(() => {
        setConnectionStatus("connecting");
    });

    connection.onreconnected(async () => {
        setConnectionStatus("online");
        await connection.invoke("JoinPublicDisplay");
    });

    connection.onclose(() => {
        setConnectionStatus("offline");
    });

    try {
        setConnectionStatus("connecting");
        await connection.start();
        await connection.invoke("JoinPublicDisplay");
        setConnectionStatus("online");
    } catch (error) {
        setConnectionStatus("offline");
        console.error("No se pudo conectar con TurnosDesk.Api.", error);

        setTimeout(startConnection, 4000);
    }
}

updateClock();
setInterval(updateClock, 1000);

loadInitialDashboardData();
startConnection();
