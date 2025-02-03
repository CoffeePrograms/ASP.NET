using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Партнеры
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PartnersController
        : ControllerBase
    {
        private readonly IRepository<Partner> _partnersRepository;
        private readonly PartnerService _partnerService;

        public PartnersController(
            IRepository<Partner> partnersRepository,
            PartnerService partnerService)
        {
            _partnersRepository = partnersRepository;
            _partnerService = partnerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PartnerResponse>>> GetPartnersAsync()
        {
            var partners = await _partnersRepository.GetAllAsync();

            var response = partners.Select(x => new PartnerResponse()
            {
                Id = x.Id,
                Name = x.Name,
                NumberIssuedPromoCodes = x.NumberIssuedPromoCodes,
                IsActive = true,
                PartnerLimits = x.PartnerLimits
                    .Select(y => new PartnerPromoCodeLimitResponse()
                    {
                        Id = y.Id,
                        PartnerId = y.PartnerId,
                        Limit = y.Limit,
                        CreateDate = y.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                        EndDate = y.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                        CancelDate = y.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
                    }).ToList()
            });

            return Ok(response);
        }
        
        [HttpGet("{id}/limits/{limitId}")]
        public async Task<ActionResult<PartnerPromoCodeLimit>> GetPartnerLimitAsync(Guid id, Guid limitId)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();
            
            var limit = partner.PartnerLimits
                .FirstOrDefault(x => x.Id == limitId);

            var response = new PartnerPromoCodeLimitResponse()
            {
                Id = limit.Id,
                PartnerId = limit.PartnerId,
                Limit = limit.Limit,
                CreateDate = limit.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                EndDate = limit.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                CancelDate = limit.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
            };
            
            return Ok(response);
        }

        
        [HttpPost("{id}/limits")]
        public async Task<IActionResult> SetPartnerPromoCodeLimitAsync(Guid id, SetPartnerPromoCodeLimitRequest request)
        {
            try
            {
                // Получаем партнера или выбрасываем исключение, если он не найден или не активен
                var partner = await _partnerService.GetPartnerOrThrowAsync(id);

                // Отключаем предыдущий лимит, если он есть
                _partnerService.DeactivatePreviousLimit(partner);

                // Создаем новый лимит
                var newLimit = _partnerService.CreateNewLimit(partner, request);

                // Добавляем новый лимит в коллекцию партнера
                partner.PartnerLimits.Add(newLimit);

                await _partnersRepository.UpdateAsync(partner);

                return CreatedAtAction(nameof(GetPartnerLimitAsync), new { id = partner.Id, limitId = newLimit.Id }, null);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AggregateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при установке лимита для партнера {ex.Message}");
                return StatusCode(500, "Произошла внутренняя ошибка сервера");
            }
        }
        
        [HttpPost("{id}/canceledLimits")]
        public async Task<IActionResult> CancelPartnerPromoCodeLimitAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);
            
            if (partner == null)
                return NotFound();
            
            //Если партнер заблокирован, то нужно выдать исключение
            if (!partner.IsActive)
                return BadRequest("Данный партнер не активен");
            
            //Отключение лимита
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x => !x.CancelDate.HasValue);
            
            if (activeLimit != null)
            {
                activeLimit.CancelDate = DateTime.Now;
            }

            await _partnersRepository.UpdateAsync(partner);

            return NoContent();
        }
    }
}