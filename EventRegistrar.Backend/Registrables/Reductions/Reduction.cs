using System;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Registrables.Reductions
{
    public class Reduction : Entity
    {
        public bool ActivatedByReduction { get; set; }
        public decimal Amount { get; set; }
        public Role? OnlyForRole { get; set; }
        //public Guid? QuestionOptionId_ActivatesReduction { get; set; }
        public Registrable Registrable { get; set; }
        public Guid RegistrableId { get; set; }
        public Guid? RegistrableId1_ReductionActivatedIfCombinedWith { get; set; }
        public Guid? RegistrableId2_ReductionActivatedIfCombinedWith { get; set; }
    }
}