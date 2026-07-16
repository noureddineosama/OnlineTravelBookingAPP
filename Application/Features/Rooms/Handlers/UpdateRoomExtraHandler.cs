using Application.Common.Interfaces;
using Application.Common.Patterns;
using Application.Features.Rooms.Commands;
using Application.Features.Rooms.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Application.Features.Rooms.Handlers
{
    public class UpdateRoomExtraHandler : IRequestHandler<UpdateRoomExtraCommand, GenericResult<UpdateRoomExtraResponseDTO>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public UpdateRoomExtraHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<GenericResult<UpdateRoomExtraResponseDTO>> Handle(UpdateRoomExtraCommand request, CancellationToken cancellationToken)
        {
            var room_instance = unitOfWork.Repository<room>();
            if (room_instance == null)
                throw new ArgumentNullException(nameof(room_instance));
            if (request.roomId <= 0)
                return await Result.FailureAsync<UpdateRoomExtraResponseDTO>("Validation Failed. ");

            if (!await room_instance.AnyAsync(predicate: op => op.id == request.roomId && op.IsDeleted == false &&
                                                                 op.status == "Active", cancellationToken))
                return await Result.FailureAsync<UpdateRoomExtraResponseDTO>("Room not found. ");

            IRepository<room_extra> room_extra_instance = unitOfWork.Repository<room_extra>();
            if (room_extra_instance == null)
                throw new ArgumentNullException(nameof(room_instance));

            //. getting tracking process for the tracking processes 
            var room_extra = await room_extra_instance.GetByIdAsync(predicate: op => op.id == request.requestDTO.id
                                                                                  && op.room_id == request.roomId, cancellationToken);

            if (room_extra == null)
                return await Result.FailureAsync<UpdateRoomExtraResponseDTO>("Room extra not found. ");

            room_extra room_extra_mapped = mapper.Map(request.requestDTO, room_extra);
            room_extra_mapped.room_id = request.roomId;

            //. --> not necessary due to when i apply the save changes will be automatically applying it in the db
            //. room_extra.Update(room_extra_mapped); 
            await unitOfWork.SaveChangesAsync();

            return await Result.SuccessAsync<UpdateRoomExtraResponseDTO>(new UpdateRoomExtraResponseDTO
            {
                Id = room_extra_mapped.id,
                Name = room_extra_mapped.name,
                Price = room_extra_mapped.price
            });
        }
    }
}
