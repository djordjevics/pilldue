using Pilldue.Business;
using Pilldue.UI.Localization;
using Pilldue.UI.Wpf;

namespace Pilldue.UI.Wpf.Tests;

public class EditorViewModelTests
{
    [Fact]
    public void Add_rejects_a_blank_name_bad_numbers_and_a_bad_date()
    {
        var interaction = new FakeUiInteraction();
        var today = new DateOnly(2026, 5, 5);
        var vm = new AddMedicationViewModel(interaction, () => today);

        vm.Name = "   ";
        vm.SaveCommand.Execute(null);
        Assert.Null(vm.Result);
        Assert.Null(vm.CloseResult);
        Assert.Contains(interaction.Messages, message => message.Message == UiLocalizer.Get("Common.NameRequired"));

        vm.Name = "Aspirin";
        vm.PackageSize = "0";
        vm.SaveCommand.Execute(null);
        Assert.Null(vm.Result);
        Assert.Contains(interaction.Messages, message => message.Message == UiLocalizer.Get("Desktop.AddInvalid"));

        vm.PackageSize = "28";
        vm.RxStart = "05/05/2026";
        vm.SaveCommand.Execute(null);
        Assert.Null(vm.Result);

        vm.RxStart = "2026-05-05";
        vm.Stock = "-1";
        vm.SaveCommand.Execute(null);
        Assert.Null(vm.Result);

        vm.Stock = "0";
        vm.SaveCommand.Execute(null);
        Assert.NotNull(vm.Result);
        Assert.Equal(true, vm.CloseResult);
        Assert.Equal(0, vm.Result.CurrentStockPills);
        Assert.Equal(today, vm.Result.PrescriptionStartDate);
        Assert.NotEqual(Guid.Empty, vm.Result.Id);
    }

    [Fact]
    public void Edit_form_keeps_the_existing_id_when_the_input_is_valid()
    {
        var existing = TestSession.Sample();
        existing.RefillDayOfMonthOverride = 4;
        var vm = new AddMedicationViewModel(new FakeUiInteraction(), () => TestSession.Today, existing);
        vm.Name = "Renamed";

        vm.SaveCommand.Execute(null);

        Assert.NotNull(vm.Result);
        Assert.Equal(existing.Id, vm.Result.Id);
        Assert.Equal(4, vm.Result.RefillDayOfMonthOverride);
        Assert.Equal("Renamed", vm.Result.Name);
    }

    [Fact]
    public void Refill_rejects_a_count_below_one_and_cancel_does_not_confirm()
    {
        var interaction = new FakeUiInteraction();
        var vm = new RefillViewModel(TestSession.Sample(), interaction);

        vm.PackageCountText = "0";
        vm.ConfirmCommand.Execute(null);
        Assert.Null(vm.CloseResult);
        Assert.Equal(0, vm.PackageCount);
        Assert.Contains(interaction.Messages, message => message.Message == UiLocalizer.Get("Common.MustBeAtLeast1"));

        vm.PackageCountText = "2";
        vm.CancelCommand.Execute(null);
        Assert.Equal(false, vm.CloseResult);
        Assert.Equal(0, vm.PackageCount);
    }

    [Fact]
    public void Skip_rejects_a_bad_count_and_a_bad_date()
    {
        var interaction = new FakeUiInteraction();
        var vm = new SkipDoseViewModel(TestSession.Sample(), interaction, () => TestSession.Today);

        vm.PillsText = "0";
        vm.ConfirmCommand.Execute(null);
        Assert.Null(vm.CloseResult);
        Assert.Equal(0, vm.PillsReturned);
        Assert.Contains(interaction.Messages, message => message.Message == UiLocalizer.Get("Common.MustBeAtLeast1"));

        vm.PillsText = "1";
        vm.DateText = "May 5";
        vm.ConfirmCommand.Execute(null);
        Assert.Null(vm.CloseResult);
        Assert.Contains(interaction.Messages, message => message.Message == UiLocalizer.Get("Common.UseDateFormat"));

        vm.DateText = "2026-05-05";
        vm.ConfirmCommand.Execute(null);
        Assert.Equal(true, vm.CloseResult);
        Assert.Equal(1, vm.PillsReturned);
        Assert.Equal(TestSession.Today, vm.Date);
    }
}
