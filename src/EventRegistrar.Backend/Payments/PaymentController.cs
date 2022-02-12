﻿using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Settlements;
using EventRegistrar.Backend.Payments.Statements;
using EventRegistrar.Backend.Payments.Unassigned;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace EventRegistrar.Backend.Payments;

public class PaymentController : Controller
{
    private readonly IEventAcronymResolver _eventAcronymResolver;
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator,
                             IEventAcronymResolver eventAcronymResolver)
    {
        _mediator = mediator;
        _eventAcronymResolver = eventAcronymResolver;
    }

    [HttpPost("api/events/{eventAcronym}/payments/{paymentId:guid}/checkIfPaymentIsSettled")]
    public async Task CheckIfPaymentIsSettled(string eventAcronym, Guid paymentId)
    {
        await _mediator.Send(new CheckIfPaymentIsSettledCommand
                             {
                                 PaymentId = paymentId
                             });
    }

    [HttpGet("api/events/{eventAcronym}/duepayments")]
    public async Task<IEnumerable<DuePaymentItem>> GetDuePayments(string eventAcronym)
    {
        return await _mediator.Send(new DuePaymentsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/payments/overview")]
    public async Task<PaymentOverview> GetPaymentOverview(string eventAcronym)
    {
        return await _mediator.Send(new PaymentOverviewQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/accounting/bank-statements")]
    public async Task<IEnumerable<BookingsOfDay>> GetPayments(string eventAcronym,
                                                              bool hideIncoming = false,
                                                              bool hideOutgoing = false,
                                                              bool hideIgnored = true,
                                                              bool hideSettled = true)
    {
        return await _mediator.Send(new BankAccountBookingsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        HideIncoming = hideIncoming,
                                        HideOutgoing = hideOutgoing,
                                        HideIgnored = hideIgnored,
                                        HideSettled = hideSettled
                                    });
    }

    [HttpGet("api/events/{eventAcronym}/payments/unassigned")]
    public async Task<IEnumerable<PaymentDisplayItem>> GetUnassignedPayments(string eventAcronym)
    {
        return await _mediator.Send(new UnassignedIncomingPaymentsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }

    [HttpPost("api/events/{eventAcronym}/registrations/{registrationId:guid}/sendReminder")]
    public async Task SendReminder(string eventAcronym, Guid registrationId, bool withholdMail)
    {
        await _mediator.Send(new SendReminderCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 RegistrationId = registrationId
                             });
    }

    [HttpPost("api/events/{eventAcronym}/payments/{paymentId:guid}/ignore")]
    public async Task IgnorePayment(string eventAcronym, Guid paymentId)
    {
        await _mediator.Send(new IgnorePaymentCommand
                             {
                                 EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                 PaymentId = paymentId
                             });
    }

    [HttpGet("api/events/{eventAcronym}/payouts/unassigned")]
    public async Task<IEnumerable<PaymentDisplayItem>> GetUnassignedPayouts(string eventAcronym)
    {
        return await _mediator.Send(new UnassignedPayoutsQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym)
                                    });
    }


    [HttpGet("api/events/{eventAcronym}/accounting/bookings-query")]
    public async Task<IEnumerable<BankBookingDisplayItem>> GetBookingsByState(string eventAcronym,
                                                                              bool hideIncoming = false,
                                                                              bool hideOutgoing = false,
                                                                              bool hideIgnored = true,
                                                                              bool hideSettled = true)
    {
        return await _mediator.Send(new BookingsByStateQuery
                                    {
                                        EventId = await _eventAcronymResolver.GetEventIdFromAcronym(eventAcronym),
                                        HideIncoming = hideIncoming,
                                        HideOutgoing = hideOutgoing,
                                        HideIgnored = hideIgnored,
                                        HideSettled = hideSettled
                                    });
    }
}