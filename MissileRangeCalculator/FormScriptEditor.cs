using CSharpScriptExecutor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissileRangeCalculator
{
    public partial class FormScriptEditor : Form
    {
        public FormScriptEditor()
        {
            InitializeComponent();
            txtScript.SetTabStopWidth(4);
        }

        public void SetScriptData(string curScript, string curScriptInfo, List<string> curScriptErrors)
        {
            txtScript.Text = curScript;
            txtScript.Select(0, 0);
            txtScriptInfo.Text = curScriptInfo;
            btnDefaultScript.Visible = (curScript == "");
            SetScriptErrors(curScriptErrors);
        }

        public void SetScriptErrors(List<string> curScriptErrors)
        {
            txtErrors.Clear();
            if (curScriptErrors != null)
            {
                foreach (string error in curScriptErrors)
                {
                    txtErrors.AppendText(error);
                    txtErrors.AppendText(Environment.NewLine);
                }
            }
        }

        public void ShowScriptStage(float time, ScriptModule scriptModule)
        {
            int scriptInfoIndex = -1;
            List<ScriptInfo> scriptInfo = ScriptInfo.AnalyzeScriptInfo(txtScriptInfo.Text, scriptModule);
            for (int i = 0; i < scriptInfo.Count; ++i)
            {
                if (scriptInfo[i].timeStart <= time && scriptInfo[i].timeEnd > time)
                {
                    scriptInfoIndex = i;
                    break;
                }
            }
            if (scriptInfoIndex >= 0)
            {
                int lineStart = txtScriptInfo.GetFirstCharIndexFromLine(scriptInfoIndex);
                if (scriptInfoIndex < scriptInfo.Count - 1)
                {
                    int lineEnd = txtScriptInfo.GetFirstCharIndexFromLine(scriptInfoIndex + 1) - Environment.NewLine.Length;
                    txtScriptInfo.Select(lineStart, lineEnd - lineStart);
                }
                else
                {
                    txtScriptInfo.Select(lineStart, txtScriptInfo.Text.Length - lineStart);
                }
            }
            else
            {
                txtScriptInfo.DeselectAll();
            }
        }

        public string GetScript()
        {
            return txtScript.Text;
        }

        public string GetScriptInfo()
        {
            return txtScriptInfo.Text;
        }
        public void SetAssemblyTreeView(Assembly compiledAssembly, Assembly[] assemblies)
        {
            treeViewClasses.Nodes.Clear();
            if (compiledAssembly != null)
            {
                AddAssemblyToTreeView(compiledAssembly, compiledAssembly.GetName().Name + " (CompiledAssembly)");
            }

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name == "MissileRangeCalculator" || assembly.GetName().Name == "CSharpScriptExecutorLibrary")
                {
                    AddAssemblyToTreeView(assembly);
                }
            }
        }

        public void AddAssemblyToTreeView(Assembly a, string overrideAssemblyName = null)
        {
            var NormalNodeFont = treeViewClasses.Font;
            var BoldNodeFont = new Font(NormalNodeFont, FontStyle.Bold);
            var ItalicNodeFont = new Font(NormalNodeFont, FontStyle.Italic);

            Type[] types = a.GetTypes();
            var assemblyNode = treeViewClasses.Nodes.Add("Assembly " + (overrideAssemblyName == null ? a.GetName().Name : overrideAssemblyName));
            foreach (Type t in types)
            {
                var classNode = assemblyNode.Nodes.Add("class " + t.Name);
                classNode.NodeFont = (t.GetCustomAttribute<DefaultClassAttribute>() != null ? BoldNodeFont : NormalNodeFont);
                var fieldNode = classNode.Nodes.Add("Fields");
                var propertyNode = classNode.Nodes.Add("Properties");
                var methodNode = classNode.Nodes.Add("Methods");
                foreach (FieldInfo fi in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    fieldNode.Nodes.Add(ReflectionUtils.GetModifierStr(fi) + ReflectionUtils.GetClassName(fi.FieldType) + " " + fi.Name).NodeFont = (fi.FieldType.GetCustomAttribute<ExchangeDataAttribute>() != null ? BoldNodeFont : NormalNodeFont);
                }
                foreach (PropertyInfo pi in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    propertyNode.Nodes.Add(ReflectionUtils.GetClassName(pi.PropertyType) + " " + pi.Name + " {" + (pi.GetMethod != null ? ReflectionUtils.GetModifierStr(pi.GetMethod) + "get;" : "") + (pi.SetMethod != null ? ReflectionUtils.GetModifierStr(pi.SetMethod) + "set;" : "") + "}");
                }
                foreach (MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    bool useBoldFont = mi.GetCustomAttribute<StartMethodAttribute>() != null || mi.GetCustomAttribute<UpdateMethodAttribute>() != null || mi.GetCustomAttribute<PostUpdateMethodAttribute>() != null;
                    methodNode.Nodes.Add(ReflectionUtils.GetModifierStr(mi) + (mi.ReturnType != null ? ReflectionUtils.GetClassName(mi.ReturnType) : "void") + " " + mi.Name + "(" + ReflectionUtils.GetMethodParameters(mi) + ")").NodeFont = (useBoldFont ? BoldNodeFont : NormalNodeFont);
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            (this.Owner as FormMain).ScriptEditorCallback(FormMain.ScriptEditorOperation.Apply);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Script Editing
        private int lastSelStart = 0;
        private void OnScriptSelectionChanged()
        {
            bool reversed = false;
            if (txtScript.SelectionLength == 0)
            {
                lastSelStart = txtScript.SelectionStart;
            }
            else
            {
                if (txtScript.SelectionStart < lastSelStart)
                {
                    reversed = true;
                }
            }
            int carotPos = (reversed ? txtScript.SelectionStart : txtScript.SelectionStart + txtScript.SelectionLength);
            int line = txtScript.GetLineFromCharIndex(carotPos) + 1;
            int column = carotPos - txtScript.GetFirstCharIndexFromLine(line - 1) + 1;
            StringBuilder sb = new StringBuilder();
            sb.Append("Line:").Append(line.ToString());
            sb.Append(' ', 9 - line.ToString().Length);
            sb.Append("Col:").Append(column.ToString());
            sb.Append(' ', 9 - column.ToString().Length);
            sb.Append("Len:").Append(txtScript.SelectionLength.ToString());

            lblInputInfo.Text = sb.ToString();
        }

        private void txtScript_MouseUp(object sender, MouseEventArgs e)
        {
            OnScriptSelectionChanged();
        }

        private void txtScript_MouseDown(object sender, MouseEventArgs e)
        {
            OnScriptSelectionChanged();
        }

        private void txtScript_MouseMove(object sender, MouseEventArgs e)
        {
            OnScriptSelectionChanged();
        }

        private void txtScript_KeyDown(object sender, KeyEventArgs e)
        {
            OnScriptSelectionChanged();
        }

        private void txtScript_KeyUp(object sender, KeyEventArgs e)
        {
            OnScriptSelectionChanged();
        }
        #endregion

        private void btnDefaultScript_Click(object sender, EventArgs e)
        {
            txtScript.Text =
@"using MissileRangeCalculator;

[DefaultClass]
public class ScriptFunctions
{
	Simulator simulator = null;
	float deltaTime = 1f / 64f;
	
	[StartMethod]
	void Start()
	{
		simulator = FormMain.singleton.simulator;
		deltaTime = simulator.accuracy;
	}
	
	[UpdateMethod]
	void Update()
	{
		simulator.UpdateFrame(deltaTime);
		simulator.ignoreUpdateFrame = true;
	}
	
	[PostUpdateMethod]
	void PostUpdate()
	{
		
	}
	
}
";
            btnDefaultScript.Visible = false;
        }

        private void txtScript_TextChanged(object sender, EventArgs e)
        {
            if (txtScript.Text != "")
                btnDefaultScript.Visible = false;
        }

        private void txtScript_SizeChanged(object sender, EventArgs e)
        {
            btnDefaultScript.Location = new Point((int)(txtScript.Left + txtScript.Width * 0.5 - btnDefaultScript.Width * 0.5), (int)(txtScript.Top + txtScript.Height * 0.5 - btnDefaultScript.Height * 0.5));
        }

        private void txtErrors_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int charIndex = txtErrors.GetCharIndexFromPosition(e.Location);
            int lineIndex = txtErrors.GetLineFromCharIndex(charIndex);
            int lineStart = txtErrors.GetFirstCharIndexFromLine(lineIndex);
            var bracketStart = txtErrors.Text.IndexOf('(', lineStart);
            var bracketEnd = txtErrors.Text.IndexOf(')', lineStart);
            if (bracketStart == -1 || bracketEnd == -1) return;
            var components = txtErrors.Text.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Split(',');
            var scriptLineIndex = int.Parse(components[0]) - 1;

            int selectionStart, selectionEnd;
            if(scriptLineIndex < txtScript.Lines.Length - 1)
            {
                selectionStart = txtScript.GetFirstCharIndexFromLine(scriptLineIndex);
                selectionEnd = txtScript.GetFirstCharIndexFromLine(scriptLineIndex + 1);
                txtScript.Select(selectionStart, selectionEnd - selectionStart - 2);
            }
            else if(scriptLineIndex == txtScript.Lines.Length - 1)
            {
                selectionStart = txtScript.GetFirstCharIndexFromLine(scriptLineIndex);
                selectionEnd = txtScript.Text.Length;
                txtScript.Select(selectionStart, selectionEnd - selectionStart);
            }
            else
            {
                selectionStart = selectionEnd = txtScript.Text.Length;
                txtScript.Select(txtScript.Text.Length, 0);
            }
            txtScript.Focus();

            lastSelStart = selectionStart;
            OnScriptSelectionChanged();
        }

        private void FormScriptEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            (this.Owner as FormMain).ScriptEditorCallback(FormMain.ScriptEditorOperation.Close);
        }

        private void splitScript_SizeChanged(object sender, EventArgs e)
        {
            txtScript_SizeChanged(sender, e);
        }
    }
}
