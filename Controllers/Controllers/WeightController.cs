﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using static VueServer.Domain.Constants;
using VueServer.Domain.Factory.Interface;
using VueServer.Services.Interface;
using System.Threading.Tasks;
using VueServer.Models;

namespace VueServer.Controllers
{
    [Route("/api/weight")]
    public class WeightController : Controller
    {
        private readonly IWeightService  _service;

        private readonly IStatusCodeFactory<IActionResult> _codeFactory;

        public WeightController(
            IWeightService service, 
            IStatusCodeFactory<IActionResult> codeFactory)
        {
            _codeFactory = codeFactory ?? throw new ArgumentNullException("Code factory is null");
            _service = service ?? throw new ArgumentNullException("Weight service is null");
        }

        [HttpGet]
        [Authorize(Roles = ROLES_ALL)]
        [Route("list")]
        public async Task<IActionResult> GetWeightHistory()
        {
            return _codeFactory.GetStatusCode(await _service.GetWeightList());
        }

        [HttpPost]
        [Authorize(Roles = ROLES_ALL)]
        [Route("add")]
        public async Task<IActionResult> AddWeight([FromBody] Weight weight)
        {
            return _codeFactory.GetStatusCode(await _service.AddWeight(weight));
        }

        [HttpDelete]
        [Authorize(Roles = ROLES_ALL)]
        [Route("delete")]
        public async Task<IActionResult> DeleteWeight(int id)
        {
            return _codeFactory.GetStatusCode(await _service.DeleteWeight(id));
        }
    }
}