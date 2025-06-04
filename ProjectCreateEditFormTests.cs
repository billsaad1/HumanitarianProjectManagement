using HumanitarianProjectManagement.Models; // Assuming this path
using System.Diagnostics; // For Debug.Assert
using System.Collections.Generic; // For List
using System.Linq;
using System; // For Console.WriteLine

namespace HumanitarianProjectManagement.Tests // Or any appropriate namespace
{
    public class ProjectCreateEditFormTests
    {
        public static void RunAllConceptualTests()
        {
            Console.WriteLine("Running ProjectCreateEditFormTests...");
            TestAddOutcome_UpdatesModel();
            TestAddOutput_UpdatesModel();
            TestAddIndicator_UpdatesModel();
            TestAddActivity_UpdatesModel();

            TestDeleteOutcome_UpdatesModel();
            TestDeleteOutput_UpdatesModel();
            TestDeleteIndicator_UpdatesModel();
            TestDeleteActivity_UpdatesModel();

            TestBudgetLineTotalCostCalculation();
            TestAddBudgetLine_UpdatesModel();
            TestDeleteBudgetLine_UpdatesModel();
            TestActivityPlanPlannedMonthsUpdate();

            Console.WriteLine("ProjectCreateEditFormTests conceptual checks completed.");
        }

        static void TestAddOutcome_UpdatesModel()
        {
            var project = new Project();
            var outcome = new Outcome { OutcomeDescription = "Test Outcome" };
            if (project.Outcomes == null) project.Outcomes = new HashSet<Outcome>();
            project.Outcomes.Add(outcome);
            Debug.Assert(project.Outcomes.Count == 1, "TestAddOutcome_UpdatesModel: Failed - Count should be 1.");
            Debug.Assert(project.Outcomes.First().OutcomeDescription == "Test Outcome", "TestAddOutcome_UpdatesModel: Failed - Description mismatch.");
        }

        static void TestAddOutput_UpdatesModel()
        {
            var outcome = new Outcome();
            var output = new Output { OutputDescription = "Test Output" };
            if (outcome.Outputs == null) outcome.Outputs = new HashSet<Output>();
            outcome.Outputs.Add(output);
            Debug.Assert(outcome.Outputs.Count == 1, "TestAddOutput_UpdatesModel: Failed - Count should be 1.");
            Debug.Assert(outcome.Outputs.First().OutputDescription == "Test Output", "TestAddOutput_UpdatesModel: Failed - Description mismatch.");
        }

        static void TestAddIndicator_UpdatesModel()
        {
            var output = new Output();
            var indicator = new ProjectIndicator { IndicatorName = "Test Indicator" };
            if (output.ProjectIndicators == null) output.ProjectIndicators = new HashSet<ProjectIndicator>();
            output.ProjectIndicators.Add(indicator);
            Debug.Assert(output.ProjectIndicators.Count == 1, "TestAddIndicator_UpdatesModel: Failed - Count should be 1.");
            Debug.Assert(output.ProjectIndicators.First().IndicatorName == "Test Indicator", "TestAddIndicator_UpdatesModel: Failed - Name mismatch.");
        }

        static void TestAddActivity_UpdatesModel()
        {
            var output = new Output();
            var activity = new Activity { ActivityDescription = "Test Activity" };
            if (output.Activities == null) output.Activities = new HashSet<Activity>();
            output.Activities.Add(activity);
            Debug.Assert(output.Activities.Count == 1, "TestAddActivity_UpdatesModel: Failed - Count should be 1.");
            Debug.Assert(output.Activities.First().ActivityDescription == "Test Activity", "TestAddActivity_UpdatesModel: Failed - Description mismatch.");
        }

        static void TestDeleteOutcome_UpdatesModel()
        {
            var project = new Project();
            var outcome1 = new Outcome { OutcomeID = 1, OutcomeDescription = "Test Outcome 1" };
            var outcome2 = new Outcome { OutcomeID = 2, OutcomeDescription = "Test Outcome 2" };
            if (project.Outcomes == null) project.Outcomes = new HashSet<Outcome>();
            project.Outcomes.Add(outcome1);
            project.Outcomes.Add(outcome2);
            project.Outcomes.Remove(outcome1);
            Debug.Assert(project.Outcomes.Count == 1, "TestDeleteOutcome_UpdatesModel: Failed - Count should be 1 after delete.");
            Debug.Assert(project.Outcomes.First().OutcomeID == 2, "TestDeleteOutcome_UpdatesModel: Failed - Remaining outcome mismatch.");
        }

        static void TestDeleteOutput_UpdatesModel()
        {
            var outcome = new Outcome();
            var output1 = new Output { OutputID = 1, OutputDescription = "Test Output 1" };
            var output2 = new Output { OutputID = 2, OutputDescription = "Test Output 2" };
            if (outcome.Outputs == null) outcome.Outputs = new HashSet<Output>();
            outcome.Outputs.Add(output1);
            outcome.Outputs.Add(output2);
            outcome.Outputs.Remove(output1);
            Debug.Assert(outcome.Outputs.Count == 1, "TestDeleteOutput_UpdatesModel: Failed - Count should be 1 after delete.");
            Debug.Assert(outcome.Outputs.First().OutputID == 2, "TestDeleteOutput_UpdatesModel: Failed - Remaining output mismatch.");
        }

        static void TestDeleteIndicator_UpdatesModel()
        {
            var output = new Output();
            var indicator1 = new ProjectIndicator { ProjectIndicatorID = 1, IndicatorName = "Test Indicator 1" };
            var indicator2 = new ProjectIndicator { ProjectIndicatorID = 2, IndicatorName = "Test Indicator 2" };
            if (output.ProjectIndicators == null) output.ProjectIndicators = new HashSet<ProjectIndicator>();
            output.ProjectIndicators.Add(indicator1);
            output.ProjectIndicators.Add(indicator2);
            output.ProjectIndicators.Remove(indicator1);
            Debug.Assert(output.ProjectIndicators.Count == 1, "TestDeleteIndicator_UpdatesModel: Failed - Count should be 1 after delete.");
            Debug.Assert(output.ProjectIndicators.First().ProjectIndicatorID == 2, "TestDeleteIndicator_UpdatesModel: Failed - Remaining indicator mismatch.");
        }

        static void TestDeleteActivity_UpdatesModel()
        {
            var output = new Output();
            var activity1 = new Activity { ActivityID = 1, ActivityDescription = "Test Activity 1" };
            var activity2 = new Activity { ActivityID = 2, ActivityDescription = "Test Activity 2" };
            if (output.Activities == null) output.Activities = new HashSet<Activity>();
            output.Activities.Add(activity1);
            output.Activities.Add(activity2);
            output.Activities.Remove(activity1);
            Debug.Assert(output.Activities.Count == 1, "TestDeleteActivity_UpdatesModel: Failed - Count should be 1 after delete.");
            Debug.Assert(output.Activities.First().ActivityID == 2, "TestDeleteActivity_UpdatesModel: Failed - Remaining activity mismatch.");
        }

        static void TestBudgetLineTotalCostCalculation()
        {
            var line = new DetailedBudgetLine { Quantity = 2, UnitCost = 10, Duration = 3, PercentageChargedToCBPF = 100 };
            line.TotalCost = line.Quantity * line.UnitCost * line.Duration * (line.PercentageChargedToCBPF / 100M);
            Debug.Assert(line.TotalCost == 60, $"TestBudgetLineTotalCostCalculation: Failed - Expected 60, Got {line.TotalCost}.");

            line.PercentageChargedToCBPF = 50;
            line.TotalCost = line.Quantity * line.UnitCost * line.Duration * (line.PercentageChargedToCBPF / 100M);
            Debug.Assert(line.TotalCost == 30, $"TestBudgetLineTotalCostCalculation: Failed - Expected 30 with 50%, Got {line.TotalCost}.");
        }

        static void TestAddBudgetLine_UpdatesModel()
        {
            var project = new Project();
            var budgetLine = new DetailedBudgetLine { Description = "Test Budget Line", UnitCost = 100 };
            if (project.DetailedBudgetLines == null) project.DetailedBudgetLines = new HashSet<DetailedBudgetLine>();
            project.DetailedBudgetLines.Add(budgetLine);
            Debug.Assert(project.DetailedBudgetLines.Count == 1, "TestAddBudgetLine_UpdatesModel: Failed - Count should be 1.");
            Debug.Assert(project.DetailedBudgetLines.First().Description == "Test Budget Line", "TestAddBudgetLine_UpdatesModel: Failed - Description mismatch.");
        }

        static void TestDeleteBudgetLine_UpdatesModel()
        {
            var project = new Project();
            var line1 = new DetailedBudgetLine { DetailedBudgetLineID = 1, Description = "Line 1" };
            var line2 = new DetailedBudgetLine { DetailedBudgetLineID = 2, Description = "Line 2" };
            if (project.DetailedBudgetLines == null) project.DetailedBudgetLines = new HashSet<DetailedBudgetLine>();
            project.DetailedBudgetLines.Add(line1);
            project.DetailedBudgetLines.Add(line2);
            project.DetailedBudgetLines.Remove(line1);
            Debug.Assert(project.DetailedBudgetLines.Count == 1, "TestDeleteBudgetLine_UpdatesModel: Failed - Count should be 1 after delete.");
            Debug.Assert(project.DetailedBudgetLines.First().DetailedBudgetLineID == 2, "TestDeleteBudgetLine_UpdatesModel: Failed - Remaining line mismatch.");
        }

        static void TestActivityPlanPlannedMonthsUpdate()
        {
            var activity = new Activity();
            string monthYearKey1 = "Jan/2023"; // Key format used in Activity Plan UI

            // Simulate adding first month
            List<string> plannedMonthsList = string.IsNullOrEmpty(activity.PlannedMonths) ? new List<string>() : activity.PlannedMonths.Split(',').ToList();
            if (!plannedMonthsList.Contains(monthYearKey1)) plannedMonthsList.Add(monthYearKey1);
            activity.PlannedMonths = string.Join(",", plannedMonthsList);
            Debug.Assert(activity.PlannedMonths == "Jan/2023", $"TestActivityPlanPlannedMonthsUpdate: Failed for single month. Got {activity.PlannedMonths}");

            // Simulate adding second month
            string monthYearKey2 = "Feb/2023";
            if (!plannedMonthsList.Contains(monthYearKey2)) plannedMonthsList.Add(monthYearKey2); // plannedMonthsList still has monthYearKey1
            activity.PlannedMonths = string.Join(",", plannedMonthsList); // Now contains both
            // Order might vary depending on List.Add behavior and string.Join, so check for both parts
            Debug.Assert(activity.PlannedMonths.Contains("Jan/2023") && activity.PlannedMonths.Contains("Feb/2023") && activity.PlannedMonths.Length == "Jan/2023,Feb/2023".Length , $"TestActivityPlanPlannedMonthsUpdate: Failed for two months. Got {activity.PlannedMonths}");

            // Simulate removing first month
            plannedMonthsList.Remove(monthYearKey1);
            activity.PlannedMonths = string.Join(",", plannedMonthsList);
            Debug.Assert(activity.PlannedMonths == "Feb/2023", $"TestActivityPlanPlannedMonthsUpdate: Failed after removing a month. Got {activity.PlannedMonths}");

            // Simulate removing last month
            plannedMonthsList.Remove(monthYearKey2);
            activity.PlannedMonths = string.Join(",", plannedMonthsList);
            Debug.Assert(activity.PlannedMonths == "", $"TestActivityPlanPlannedMonthsUpdate: Failed after removing all months. Got {activity.PlannedMonths}");
        }
    }
}
