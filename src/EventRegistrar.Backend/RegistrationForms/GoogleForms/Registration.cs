﻿namespace EventRegistrar.Backend.RegistrationForms.GoogleForms;

public class Registration
{
    public string Email { get; set; }
    public IEnumerable<ResponseData> Responses { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}