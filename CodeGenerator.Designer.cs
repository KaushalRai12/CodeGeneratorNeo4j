namespace CodeGeneratorNeo4j
{
    partial class CodeGenerator
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnTestConnection = new Button();
            btnGenerateClasses = new Button();
            txtOutput = new TextBox();
            txtQuery = new TextBox();
            txtCodeSection = new TextBox();
            GenerateCode = new Button();
            SuspendLayout();
            // 
            // btnTestConnection
            // 
            btnTestConnection.Location = new Point(12, 508);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(160, 23);
            btnTestConnection.TabIndex = 0;
            btnTestConnection.Text = "TestConnection";
            btnTestConnection.UseVisualStyleBackColor = true;
            btnTestConnection.Click += btnTestConnection_Click;
            // 
            // btnGenerateClasses
            // 
            btnGenerateClasses.Location = new Point(204, 508);
            btnGenerateClasses.Name = "btnGenerateClasses";
            btnGenerateClasses.Size = new Size(198, 23);
            btnGenerateClasses.TabIndex = 1;
            btnGenerateClasses.Text = "Generate Classes";
            btnGenerateClasses.UseVisualStyleBackColor = true;
            btnGenerateClasses.Click += btnGenerateClasses_Click;
            // 
            // txtOutput
            // 
            txtOutput.Location = new Point(443, 12);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.Size = new Size(687, 472);
            txtOutput.TabIndex = 2;
            // 
            // txtQuery
            // 
            txtQuery.Location = new Point(24, 21);
            txtQuery.Multiline = true;
            txtQuery.Name = "txtQuery";
            txtQuery.Size = new Size(413, 463);
            txtQuery.TabIndex = 3;
            // 
            // txtCodeSection
            // 
            txtCodeSection.Location = new Point(1152, 12);
            txtCodeSection.Multiline = true;
            txtCodeSection.Name = "txtCodeSection";
            txtCodeSection.Size = new Size(536, 472);
            txtCodeSection.TabIndex = 4;
            // 
            // GenerateCode
            // 
            GenerateCode.Location = new Point(1152, 508);
            GenerateCode.Name = "GenerateCode";
            GenerateCode.Size = new Size(178, 23);
            GenerateCode.TabIndex = 5;
            GenerateCode.Text = "GenerateCode";
            GenerateCode.UseVisualStyleBackColor = true;
            GenerateCode.Click += GenerateCode_Click;
            // 
            // CodeGenerator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1742, 660);
            Controls.Add(GenerateCode);
            Controls.Add(txtCodeSection);
            Controls.Add(txtQuery);
            Controls.Add(txtOutput);
            Controls.Add(btnGenerateClasses);
            Controls.Add(btnTestConnection);
            Name = "CodeGenerator";
            Text = "CodeGenerator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private Button btnTestConnection;
        private Button btnGenerateClasses;
        private TextBox txtOutput;
        private TextBox txtQuery;
        private TextBox txtCodeSection;
        private Button GenerateCode;
    }
}