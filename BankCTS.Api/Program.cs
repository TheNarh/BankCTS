
using BankCTS.Api.Contracts;
using BankCTS.Api.Domain;
using BankCTS.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankCtsDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("BankCtsDatabase")));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// CREATE A CHEQUE
app.MapPost("/api/cheques", async (
    CreateChequeRequest request,
    BankCtsDbContext db) =>
{
    var cheque = new Cheque
    {
        ChequeDate = request.ChequeDate,
        PresentationDate = request.PresentationDate,
        DrawerName = request.DrawerName?.Trim() ?? string.Empty,
        PayeeName = request.PayeeName?.Trim() ?? string.Empty
    };

    try
    {
        cheque.SetChequeNumber(request.ChequeNumber);
        cheque.SetAccountNumber(request.AccountNumber);
        cheque.SetAmount(request.Amount);

        // Validate the cheque before accepting it.
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

    // Add the cheque to the database.
    await db.Cheques.AddAsync(cheque);

    // Permanently save the changes.
    await db.SaveChangesAsync();

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

// GET ALL CHEQUES
app.MapGet("/api/cheques", async (BankCtsDbContext db) =>
{
    var cheques = await db.Cheques
        .AsNoTracking()
        .ToListAsync();

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

// GET A SINGLE CHEQUE
app.MapGet("/api/cheques/{id:guid}", async (
    Guid id,
    BankCtsDbContext db) =>
{
    var cheque = await db.Cheques
        .AsNoTracking()
        .FirstOrDefaultAsync(cheque => cheque.Id == id);

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
