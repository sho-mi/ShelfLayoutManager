using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ShelfLayout.Core.Entities;
using ShelfLayout.Core.Interfaces;

namespace ShelfLayout.Application.Services
{
    public class ShelfLayoutService
    {
        private readonly ISkuRepository _skuRepository;
        private readonly IShelfRepository _shelfRepository;
        private readonly Subject<Sku> _skuAddedSubject = new Subject<Sku>();
        private readonly Subject<Sku> _skuMovedSubject = new Subject<Sku>();
        private readonly Subject<string> _skuRemovedSubject = new Subject<string>();

        public IObservable<Sku> SkuAdded => _skuAddedSubject;
        public IObservable<Sku> SkuMoved => _skuMovedSubject;
        public IObservable<string> SkuRemoved => _skuRemovedSubject;

        public ShelfLayoutService(ISkuRepository skuRepository, IShelfRepository shelfRepository)
        {
            _skuRepository = skuRepository;
            _shelfRepository = shelfRepository;
        }

        public async Task<IEnumerable<Cabinet>> GetAllCabinetsAsync()
        {
            return await _shelfRepository.GetAllCabinetsAsync();
        }

        public async Task AddSkuToLaneAsync(string janCode, int cabinetNumber, int rowNumber, int laneNumber)
        {
            var sku = await _skuRepository.GetByJanCodeAsync(janCode);
            if (sku == null)
                throw new ArgumentException($"SKU with JanCode {janCode} not found");

            var lane = await _shelfRepository.GetLaneAsync(cabinetNumber, rowNumber, laneNumber);
            if (lane == null)
                throw new ArgumentException($"Lane not found");

            if (lane.JanCode != null)
                throw new InvalidOperationException("Lane is already occupied");

            lane.JanCode = janCode;
            await _shelfRepository.UpdateLaneAsync(cabinetNumber, rowNumber, lane);
            _skuAddedSubject.OnNext(sku);
        }

        public async Task MoveSkuAsync(string janCode, int fromCabinet, int fromRow, int fromLane, 
            int toCabinet, int toRow, int toLane)
        {
            var sku = await _skuRepository.GetByJanCodeAsync(janCode);
            if (sku == null)
                throw new ArgumentException($"SKU with JanCode {janCode} not found");

            var fromLaneObj = await _shelfRepository.GetLaneAsync(fromCabinet, fromRow, fromLane);
            var toLaneObj = await _shelfRepository.GetLaneAsync(toCabinet, toRow, toLane);

            if (fromLaneObj == null || toLaneObj == null)
                throw new ArgumentException("Invalid lane specified");

            if (fromLaneObj.JanCode != janCode)
                throw new InvalidOperationException("SKU not found in source lane");

            if (toLaneObj.JanCode != null)
                throw new InvalidOperationException("Destination lane is already occupied");

            fromLaneObj.JanCode = null;
            toLaneObj.JanCode = janCode;

            await _shelfRepository.UpdateLaneAsync(fromCabinet, fromRow, fromLaneObj);
            await _shelfRepository.UpdateLaneAsync(toCabinet, toRow, toLaneObj);

            _skuMovedSubject.OnNext(sku);
        }

        public async Task RemoveSkuAsync(string janCode, int cabinetNumber, int rowNumber, int laneNumber)
        {
            var lane = await _shelfRepository.GetLaneAsync(cabinetNumber, rowNumber, laneNumber);
            if (lane == null)
                throw new ArgumentException("Lane not found");

            if (lane.JanCode != janCode)
                throw new InvalidOperationException("SKU not found in specified lane");

            lane.JanCode = null;
            await _shelfRepository.UpdateLaneAsync(cabinetNumber, rowNumber, lane);
            _skuRemovedSubject.OnNext(janCode);
        }
    }
} 