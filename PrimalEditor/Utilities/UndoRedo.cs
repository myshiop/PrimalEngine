using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEditor.Utilities
{
    interface IUndoRedo
    {
        string Name { get; }

        void Undo();
        void Redo();
    }

    class UndoRedo
    {
        private readonly ObservableCollection<IUndoRedo> m_redoList = new ObservableCollection<IUndoRedo>();
        private readonly ObservableCollection<IUndoRedo> m_undoList = new ObservableCollection<IUndoRedo>();

        public ReadOnlyObservableCollection<IUndoRedo> ReDoList { get; }
        public ReadOnlyObservableCollection<IUndoRedo> UnDoList { get; }

        public void Reset()
        {
            m_redoList.Clear();
            m_undoList.Clear();
        }

        public void Add(IUndoRedo cmd)
        {
            m_undoList.Add(cmd);
            m_redoList.Clear();
        }

        public void UnDo()
        {
            if (m_undoList.Any()){
                var cmd = m_undoList.Last();
                m_undoList.RemoveAt(m_undoList.Count - 1);
                cmd.Undo();
                m_redoList.Insert(0, cmd);
            }
        }

        public void ReDo()
        {
            if (m_redoList.Any())
            {
                var cmd = m_redoList.First();
                m_redoList.RemoveAt(0);
                cmd.Redo();
                m_undoList.Add(cmd);
            }
        }

        public UndoRedo()
        {
            ReDoList = new ReadOnlyObservableCollection<IUndoRedo>(m_redoList);
            UnDoList = new ReadOnlyObservableCollection<IUndoRedo>(m_undoList);
        }
    }

    public class UndoReoAction : IUndoRedo
    {
        private Action m_undoAction;
        private Action m_redoAction;
        public string Name { get; }

        public void Redo() => m_redoAction();

        public void Undo() => m_undoAction();
    }
}
