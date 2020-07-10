using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Factory.Interface;
using VueServer.Models.Chat;
using VueServer.Models.Request;
using VueServer.Services.Interface;
using static VueServer.Domain.Constants.Authentication;

namespace VueServer.Controllers
{
    [Route("/api/chat")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public ChatController(IChatService chatService, IStatusCodeFactory<IActionResult> factory)
        {
            _chatService = chatService ?? throw new ArgumentNullException("Chat service is null");
            _codeFactory = factory ?? throw new ArgumentNullException("Code factory is null");
        }

        [Route("conversation/start")]
        [HttpPost]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.StartConversation(request));
        }

        [Route("conversation/get/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetConversation(id));
        }

        [Route("conversation/notifications/get-all")]
        [HttpGet]
        public async Task<IActionResult> GetNewMessageNotifications()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetNewMessageNotifications());
        }        

        [Route("conversation/get-all")]
        [HttpGet]
        public async Task<IActionResult> GetAllConversations()
        {
            return _codeFactory.GetStatusCode(await _chatService.GetAllConversations());
        }

        [Route("conversation/update-title/{conversationId}")]
        [HttpPost]
        public async Task<IActionResult> UpdateConversationTitle(long conversationId, [FromBody] UpdateConversationTitleRequest request)
        {
            return _codeFactory.GetStatusCode(await _chatService.UpdateConversationTitle(conversationId, request?.Title));
        }

        [Route("conversation/delete/{conversationId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteConversation(long conversationId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteConversation(conversationId));
        }

        [Route("conversation/get/messages/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetMessagesForConversation(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessagesForConversation(id));
        }

        [Route("message/delete/{messageId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.DeleteMessage(messageId));
        }

        [Route("message/get/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetMessage(long id)
        {
            return _codeFactory.GetStatusCode(await _chatService.GetMessage(id));
        }

        [Route("message/send")]
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] ChatMessage message)
        {
            return _codeFactory.GetStatusCode(await _chatService.AddMessage(message));
        }

        [Route("message/read/{conversationId}/{messageId}")]
        [HttpPut]
        public async Task<IActionResult> ReadMessage(long conversationId, long messageId)
        {
            return _codeFactory.GetStatusCode(await _chatService.ReadMessage(conversationId, messageId));
        }
    }
}
