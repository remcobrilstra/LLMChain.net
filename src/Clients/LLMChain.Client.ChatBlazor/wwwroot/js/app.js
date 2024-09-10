function toggleNav() {

    if (document.body.classList.contains("menu-collapse")) {
        document.body.classList.remove("menu-collapse");
    }
    else {
        document.body.classList.add("menu-collapse");
    }
}

const chatInput = document.querySelector("#chat-input");
const sendButton = document.querySelector("#send-btn");
const chatContainer = document.querySelector(".chat-container");
const themeButton = document.querySelector("#theme-btn");
const deleteButton = document.querySelector("#delete-btn");

let userText = null;

//const loadDataFromLocalstorage = () => {
//    // Load saved chats and theme from local storage and apply/add on the page
//    const themeColor = localStorage.getItem("themeColor");

//    document.body.classList.toggle("light-mode", themeColor === "light_mode");
//    themeButton.innerText = document.body.classList.contains("light-mode") ? "dark_mode" : "light_mode";

//    const defaultText = `<div class="default-text">
//                              <h1>ChatGPT Clone</h1>
//                              <p>Start a conversation and explore the power of AI.<br> Your chat history will be displayed here.</p>
//                          </div>`
//}

//const createChatElement = (content, className) => {
//     Create new div and apply chat, specified class and set html content of div
//    const chatDiv = document.createElement("div");
//    chatDiv.classList.add("chat", className);
//    chatDiv.innerHTML = content;
//    return chatDiv; // Return the created chat div
//}

//const getChatResponse = async (incomingChatDiv) => {
//    const API_URL = "https://api.openai.com/v1/chat/completions";
//    var API_KEY = localStorage.getItem("openAI-key");
//    var API_MODEL = localStorage.getItem("openAI-model");
//    const pElement = document.createElement("p");

//     Define the properties and data for the API request
//    const requestOptions = {
//        method: "POST",
//        headers: {
//            "Content-Type": "application/json",
//            "Authorization": `Bearer ${API_KEY}`
//        },
//        body: JSON.stringify({
//            model: API_MODEL,
//            messages: getMessageHistory(getCurrentConversationId())
//        })
//    }

//     Send POST request to API, get response and set the reponse as paragraph element text
//    try {
//        const response = await (await fetch(API_URL, requestOptions)).json();
//        pElement.textContent = response.choices[0].message.content.trim();
//        pushMessage(getCurrentConversationId(), response.choices[0].message.role, response.choices[0].message.content.trim());
//    } catch (error) { // Add error class to the paragraph element and set error text
//        pElement.classList.add("error");
//        pElement.textContent = "Oops! Something went wrong while retrieving the response. Please try again.";
//    }

//     Remove the typing animation, append the paragraph element and save the chats to local storage
//    incomingChatDiv.querySelector(".typing-animation").remove();
//    incomingChatDiv.querySelector(".chat-details").appendChild(pElement);
//    localStorage.setItem("all-chats", chatContainer.innerHTML);
//    chatContainer.scrollTo(0, chatContainer.scrollHeight);
//}

//const copyResponse = (copyBtn) => {
//     Copy the text content of the response to the clipboard
//    const reponseTextElement = copyBtn.parentElement.querySelector("p");
//    navigator.clipboard.writeText(reponseTextElement.textContent);
//    copyBtn.textContent = "done";
//    setTimeout(() => copyBtn.textContent = "content_copy", 1000);
//}

//const showTypingAnimation = () => {
//    var incomingChatDiv = addSystemMessageToUI("", true);
//    getChatResponse(incomingChatDiv);
//}

//const handleOutgoingChat = () => {
//    userText = chatInput.value.trim(); // Get chatInput value and remove extra spaces
//    if (!userText) return; // If chatInput is empty return from here

//    pushMessage(getCurrentConversationId(), 'user', userText);

//     Clear the input field and reset its height
//    chatInput.value = "";
//    chatInput.style.height = `${initialInputHeight}px`;

//    addUserMessageToUI(userText);
//    setTimeout(showTypingAnimation, 500);
//}

//deleteButton.addEventListener("click", () => {
//    // Remove the chats from local storage and call loadDataFromLocalstorage function
//    if (confirm("Are you sure you want to delete this conversation?")) {
//        deleteConversation(getCurrentConversationId());
//    }
//});

//themeButton.addEventListener("click", () => {
//    // Toggle body's class for the theme mode and save the updated theme to the local storage 
//    document.body.classList.toggle("light-mode");
//    localStorage.setItem("themeColor", themeButton.innerText);
//    themeButton.innerText = document.body.classList.contains("light-mode") ? "dark_mode" : "light_mode";
//});

//const initialInputHeight = chatInput.scrollHeight;

//chatInput.addEventListener("input", () => {
//    // Adjust the height of the input field dynamically based on its content
//    chatInput.style.height = `${initialInputHeight}px`;
//    chatInput.style.height = `${chatInput.scrollHeight}px`;
//});

//chatInput.addEventListener("keydown", (e) => {
//    // If the Enter key is pressed without Shift and the window width is larger 
//    // than 800 pixels, handle the outgoing chat
//    if (e.key === "Enter" && !e.shiftKey && window.innerWidth > 800) {
//        e.preventDefault();
//        handleOutgoingChat();
//    }
//});

//function saveSettings() {
//    localStorage.setItem("openAI-key", document.getElementById("openAI-key").value);
//    localStorage.setItem("openAI-model", document.getElementById("openAI-model").value);
//}

function getModalAnchors() {

    const modallinks = document.querySelectorAll('[data-modal]');
    modallinks.forEach(item => {

        item.onclick = function (e) {
            var target = document.getElementById(item.attributes['data-modal'].value);
            var action = item.attributes['data-toggle'].value;

            if (action == 'show') {
                target.style.display = "block";
            }
            else {
                target.style.display = "none";
            }

        }

    });

}

//function pushMessage(convId, actor, text) {
//    var temp = JSON.parse(localStorage.getItem("chat-history-" + convId));
//    if (temp == undefined) { temp = []; }

//    var convertedText = text.replace(/(?:\r\n|\r|\n)/g, "<br>");


//    temp.push({
//        role: actor,
//        content: convertedText
//    });

//    localStorage.setItem("chat-history-" + convId, JSON.stringify(temp));
//}

//function loadHistory() {

//    var chatContainer = document.querySelector(".chat-container");
//    chatContainer.innerHTML = "";

//    var history = getMessageHistory(getCurrentConversationId());

//    history.forEach(msg => {

//        if (msg.role == "user") {
//            addUserMessageToUI(msg.content);
//        }
//        else if (msg.role == "assistant") {
//            addSystemMessageToUI(msg.content, false);

//        }
//    });
//}

//function addUserMessageToUI(text) {
//    var chatContainer = document.querySelector(".chat-container");
//    const html = `<div class="chat-content">
//                    <div class="chat-details">
//                        <img src="https://avatar.iran.liara.run/public/40" alt="user-img">
//                        <p>${text}</p>
//                    </div>
//                </div>`;
//    const outgoingChatDiv = createChatElement(html, "outgoing");
//    chatContainer.querySelector(".default-text")?.remove();
//    chatContainer.appendChild(outgoingChatDiv);
//    chatContainer.scrollTo(0, chatContainer.scrollHeight);

//    return outgoingChatDiv;
//}

//function addSystemMessageToUI(text, typing = false) {
//    var chatContainer = document.querySelector(".chat-container");

//    var content = '<p>' + text + '</p>';

//    if (typing) {
//        content = `<div class="typing-animation">
//                            <div class="typing-dot" style="--delay: 0.2s"></div>
//                            <div class="typing-dot" style="--delay: 0.3s"></div>
//                            <div class="typing-dot" style="--delay: 0.4s"></div>
//                        </div>`;
//    }
//    const html = '<div class="chat-content"><div class="chat-details"><img src="https://avatar.iran.liara.run/public/5" alt="chatbot-img">' + content + '</div><span onclick="copyResponse(this)" class="material-symbols-rounded">content_copy</span></div>';
//    const incomingChatDiv = createChatElement(html, "incoming");
//    chatContainer.querySelector(".default-text")?.remove();
//    chatContainer.appendChild(incomingChatDiv);
//    chatContainer.scrollTo(0, chatContainer.scrollHeight);

//    return incomingChatDiv;
//}

//function getMessageHistory(conversationId) {
//    return JSON.parse(localStorage.getItem("chat-history-" + conversationId));
//}

//function AddConversation() {

//    var name = document.getElementById("new-conv-name").value;
//    document.getElementById("new-conv-name").value = "";

//    var sysprompt = document.getElementById("new-conv-prompt").value;
//    document.getElementById("new-conv-prompt").value = "";

//    var convs = GetConversations();
//    var key = randomString(name);
//    convs.push({
//        id: key,
//        name: name,
//        SystemPrompt: sysprompt
//    });
//    localStorage.setItem("conversations", JSON.stringify(convs));


//    pushMessage(key, "system", sysprompt);


//    setCurrentConvesation(key);
//    UpdateConvList();
//}

//function GetConversations() {
//    var temp = localStorage.getItem("conversations");

//    if (temp == undefined) {
//        return [];
//    }
//    else {
//        return JSON.parse(temp);
//    }

//}

//function GetConversationById(Id) {
//    var convs = GetConversations();

//    return convs.find((conv) => conv.id == Id);

//}

//function getCurrentConversationId() {

//    return localStorage.getItem("current-conversation");
//}

//function setCurrentConvesation(id) {

//    var conv = GetConversationById(id);

//    localStorage.setItem("current-conversation", id);
//    loadHistory();
//}

//function randomString(str) {
//    var uid = '';
//    str = str.replace(/\s/g, ''); // Remove spaces
//    for (var i = str.length; i > 0; --i) {
//        uid += str[Math.round(Math.random() * (str.length - 1))];
//    }
//    return uid;
//}

//function UpdateConvList() {

//    var convsersations = GetConversations();
//    var container = document.getElementById('conversations');

//    while (container.childElementCount > 0) {
//        container.removeChild(container.children[0]);
//    }

//    convsersations.forEach(item => {
//        var anchor = document.createElement('a');
//        anchor.href = 'javascript:setCurrentConvesation("' + item.id + '")';
//        anchor.textContent = item.name;
//        anchor.target = '_blank'; // Opens the link in a new tab
//        container.appendChild(anchor);
//        container.appendChild(document.createElement('br')); // Adds a line break after each anchor
//    });
//}



document.addEventListener("DOMContentLoaded", function () {

    //loadDataFromLocalstorage();
    //sendButton.addEventListener("click", handleOutgoingChat);

    // When the user clicks anywhere outside of the modal, close it
    //window.onclick = function (event) {
    //    if (event.target.classList.contains('modal')) {
    //        event.target.style.display = "none";
    //    }
    //}

    getModalAnchors();
    //UpdateConvList();
});