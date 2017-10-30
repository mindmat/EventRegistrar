using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Registrables
{
    public class Reduction : Entity
    {
        public Guid RegistrableId { get; set; }
        public Guid? RegistrableId1_ReductionActivatedIfCombinedWith { get; set; }
        public Guid? RegistrableId2_ReductionActivatedIfCombinedWith { get; set; }
        public Guid? QuestionOptionId_ActivatesReduction { get; set; }
        public decimal Amount { get; set; }
    }
}