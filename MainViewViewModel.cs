using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPIUI7._1.Wrappers;
using RevitAPIUILibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIUI7._1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;
        public List<ElementType> TitleBlocks { get; } = new List<ElementType>();
        public ElementType SelectedTitleBlock { get; set; }
        public List<ViewPlan> Views { get; } = new List<ViewPlan>();
        public ViewPlan SelectedViewPlan { get; set; }
        public string SheetCount { get; set; }
        public string DesignedBy { get; set; }
        public DelegateCommand SaveCommand { get; }






        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;
            TitleBlocks = SheetUtils.GetTitleBlocks(_doc);
            Views = ViewsUtils.GetFloorPlanViews(_doc);
            SaveCommand = new DelegateCommand(OnSaveCommand);

        }

        private void OnSaveCommand()
        {
            
            if (SelectedTitleBlock == null || SelectedViewPlan == null || SheetCount == string.Empty)
                return;

            var num = Convert.ToInt32(SheetCount);

            for (int i = 0; i < num; i++)
            {
                using (var ts = new Transaction(_doc, "Create Sheet"))
                {
                    ts.Start();
                    ViewSheet newSheet = ViewSheet.Create(_doc, SelectedTitleBlock.Id);
                    //newSheet.Name = "Sheet1";
                    //newSheet.SheetNumber = "A-01";
                    ElementId duplicatePlan = SelectedViewPlan.Duplicate(ViewDuplicateOption.Duplicate);
                    UV location = new UV((newSheet.Outline.Max.U - newSheet.Outline.Min.U) / 2, (newSheet.Outline.Max.V - newSheet.Outline.Min.V) / 2);
                    Viewport newViewPort = Viewport.Create(_doc, newSheet.Id, duplicatePlan, new XYZ(location.U, location.V, 0));
                    Parameter designedBy = newSheet.get_Parameter(BuiltInParameter.SHEET_DESIGNED_BY);
                    designedBy.Set(DesignedBy);
                    ts.Commit();
                }
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        //public event EventHandler HideRequest;
        //private void RaiseHideRequest()
        //{
        //    HideRequest?.Invoke(this, EventArgs.Empty);
        //}

        //public event EventHandler ShowRequest;
        //private void RaiseShowRequest()
        //{
        //    ShowRequest?.Invoke(this, EventArgs.Empty);
        //}
    }
}
