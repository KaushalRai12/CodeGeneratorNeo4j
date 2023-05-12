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
        
            SuspendLayout();
            // 
            // btnTestConnection
            // 
            btnTestConnection.Location = new Point(226, 340);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(160, 23);
            btnTestConnection.TabIndex = 0;
            btnTestConnection.Text = "TestConnection";
            btnTestConnection.UseVisualStyleBackColor = true;
            btnTestConnection.Click += btnTestConnection_Click;
           
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1043, 543);
          
            Controls.Add(btnTestConnection);
            Name = "CodeGenerator";
            Text = "CodeGenerator";
            ResumeLayout(false);
        }

        #endregion


        private Button btnTestConnection;
        
    }
}