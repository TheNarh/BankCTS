
using BankCTS.Api.Contracts;
using BankCTS.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Temporary in-memory storage.
// Data will be lost when the application restarts.
var cheques = new List<Cheque>();

app.MapPost("/api/cheques", (CreateChequeRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.ChequeNumber) ||
        string.IsNullOrWhiteSpace(request.AccountNumber) ||
        string.IsNullOrWhiteSpace(request.DrawerName) ||
        string.IsNullOrWhiteSpace(request.PayeeName))
    {
        return Results.BadRequest(new
        {
            message = "Cheque number, account number, drawer name, and payee name are required."
        });
    }

    var cheque = new Cheque
    {
        ChequeDate = request.ChequeDate,
        PresentationDate = request.PresentationDate,
        DrawerName = request.DrawerName.Trim(),
        PayeeName = request.PayeeName.Trim()
    };

    cheque.SetChequeNumber(request.ChequeNumber);
    cheque.SetAccountNumber(request.AccountNumber);
    cheque.SetAmount(request.Amount);

    try
    {
        // Validate the cheque before accepting it for processing.
        cheque.TransitionTo(ChequeStatus.Validated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }

    cheque.Id = Guid.NewGuid();
    cheques.Add(cheque);

    return Results.Created(
        $"/api/cheques/{cheque.Id}",
        new
        {
            cheque.Id,
            cheque.ChequeNumber,
            cheque.AccountNumber,
            cheque.Amount,
            cheque.Currency,
            cheque.ChequeDate,
            cheque.PresentationDate,
            cheque.DrawerName,
            cheque.PayeeName,
            Status = cheque.Status.ToString()
        });
})
.WithName("CreateCheque");

app.MapGet("/api/cheques", () =>
{
    return Results.Ok(cheques.Select(cheque => new
    {
        cheque.Id,
        cheque.ChequeNumber,
        cheque.AccountNumber,
        cheque.Amount,
        cheque.Currency,
        cheque.ChequeDate,
        cheque.PresentationDate,
        cheque.DrawerName,
        cheque.PayeeName,
        Status = cheque.Status.ToString()
    }));
})
.WithName("GetAllCheques");

app.MapGet("/api/cheques/{id:guid}", (Guid id) =>
{
    var cheque = cheques.Find(cheque => cheque.Id == id);

    if (cheque is null)
    {
        return Results.NotFound(new
        {
            message = $"Cheque with ID {id} was not found."
        });
    }

    return Results.Ok(new
    {
        cheque.Id,
        cheque.ChequeNumber,
        cheque.AccountNumber,
        cheque.Amount,
        cheque.Currency,
        cheque.ChequeDate,
        cheque.PresentationDate,
        cheque.DrawerName,
        cheque.PayeeName,
        Status = cheque.Status.ToString()
    });
});


app.Run();
