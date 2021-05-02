namespace BusinessCentralTNTConnector
{
    partial class OrdersForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.signUpButton = new System.Windows.Forms.Button();
            this.shippingPostalAddressBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.numberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToName2DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToAddress2DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToPostCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToCityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipmentMethodCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shippingAgentCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shippingNoDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shippingNoSeriesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.packageTrackingNoDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shipmentDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.postingDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.requestedDeliveryDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.shippedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SumStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.auxiliaryIndex1DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shippingPostalAddressBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idDataGridViewTextBoxColumn,
            this.shipToNameDataGridViewTextBoxColumn,
            this.numberDataGridViewTextBoxColumn,
            this.shipToName2DataGridViewTextBoxColumn,
            this.shipToAddressDataGridViewTextBoxColumn,
            this.shipToAddress2DataGridViewTextBoxColumn,
            this.shipToPostCodeDataGridViewTextBoxColumn,
            this.shipToCityDataGridViewTextBoxColumn,
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn,
            this.shipmentMethodCodeDataGridViewTextBoxColumn,
            this.shippingAgentCodeDataGridViewTextBoxColumn,
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn,
            this.shippingNoDataGridViewTextBoxColumn,
            this.shippingNoSeriesDataGridViewTextBoxColumn,
            this.packageTrackingNoDataGridViewTextBoxColumn,
            this.shipmentDateDataGridViewTextBoxColumn,
            this.postingDateDataGridViewTextBoxColumn,
            this.requestedDeliveryDateDataGridViewTextBoxColumn,
            this.shippedDataGridViewTextBoxColumn,
            this.SumStatus,
            this.auxiliaryIndex1DataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.shippingPostalAddressBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(2, 49);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(733, 450);
            this.dataGridView1.TabIndex = 0;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(13, 13);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(242, 22);
            this.dateTimePicker1.TabIndex = 1;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // signUpButton
            // 
            this.signUpButton.Location = new System.Drawing.Point(540, 15);
            this.signUpButton.Name = "signUpButton";
            this.signUpButton.Size = new System.Drawing.Size(180, 30);
            this.signUpButton.TabIndex = 2;
            this.signUpButton.Text = "Sign Up Order With TNT";
            this.signUpButton.UseVisualStyleBackColor = true;
            this.signUpButton.Click += new System.EventHandler(this.signUpButton_Click);
            // 
            // shippingPostalAddressBindingSource
            // 
            this.shippingPostalAddressBindingSource.AllowNew = true;
            this.shippingPostalAddressBindingSource.DataSource = typeof(Entocare.NAV.ShippingPostalAddress);
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Id";
            this.idDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.Visible = false;
            this.idDataGridViewTextBoxColumn.Width = 125;
            // 
            // shipToNameDataGridViewTextBoxColumn
            // 
            this.shipToNameDataGridViewTextBoxColumn.DataPropertyName = "ShipToName";
            this.shipToNameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.shipToNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToNameDataGridViewTextBoxColumn.Name = "shipToNameDataGridViewTextBoxColumn";
            this.shipToNameDataGridViewTextBoxColumn.Width = 250;
            // 
            // numberDataGridViewTextBoxColumn
            // 
            this.numberDataGridViewTextBoxColumn.DataPropertyName = "Number";
            this.numberDataGridViewTextBoxColumn.HeaderText = "OrderNr";
            this.numberDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.numberDataGridViewTextBoxColumn.Name = "numberDataGridViewTextBoxColumn";
            this.numberDataGridViewTextBoxColumn.Width = 75;
            // 
            // shipToName2DataGridViewTextBoxColumn
            // 
            this.shipToName2DataGridViewTextBoxColumn.DataPropertyName = "ShipToName2";
            this.shipToName2DataGridViewTextBoxColumn.HeaderText = "ShipToName2";
            this.shipToName2DataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToName2DataGridViewTextBoxColumn.Name = "shipToName2DataGridViewTextBoxColumn";
            this.shipToName2DataGridViewTextBoxColumn.Visible = false;
            this.shipToName2DataGridViewTextBoxColumn.Width = 125;
            // 
            // shipToAddressDataGridViewTextBoxColumn
            // 
            this.shipToAddressDataGridViewTextBoxColumn.DataPropertyName = "ShipToAddress";
            this.shipToAddressDataGridViewTextBoxColumn.HeaderText = "ShipToAddress";
            this.shipToAddressDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToAddressDataGridViewTextBoxColumn.Name = "shipToAddressDataGridViewTextBoxColumn";
            this.shipToAddressDataGridViewTextBoxColumn.Visible = false;
            this.shipToAddressDataGridViewTextBoxColumn.Width = 125;
            // 
            // shipToAddress2DataGridViewTextBoxColumn
            // 
            this.shipToAddress2DataGridViewTextBoxColumn.DataPropertyName = "ShipToAddress2";
            this.shipToAddress2DataGridViewTextBoxColumn.HeaderText = "ShipToAddress2";
            this.shipToAddress2DataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToAddress2DataGridViewTextBoxColumn.Name = "shipToAddress2DataGridViewTextBoxColumn";
            this.shipToAddress2DataGridViewTextBoxColumn.Visible = false;
            this.shipToAddress2DataGridViewTextBoxColumn.Width = 125;
            // 
            // shipToPostCodeDataGridViewTextBoxColumn
            // 
            this.shipToPostCodeDataGridViewTextBoxColumn.DataPropertyName = "ShipToPostCode";
            this.shipToPostCodeDataGridViewTextBoxColumn.HeaderText = "ShipToPostCode";
            this.shipToPostCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToPostCodeDataGridViewTextBoxColumn.Name = "shipToPostCodeDataGridViewTextBoxColumn";
            this.shipToPostCodeDataGridViewTextBoxColumn.Visible = false;
            this.shipToPostCodeDataGridViewTextBoxColumn.Width = 125;
            // 
            // shipToCityDataGridViewTextBoxColumn
            // 
            this.shipToCityDataGridViewTextBoxColumn.DataPropertyName = "ShipToCity";
            this.shipToCityDataGridViewTextBoxColumn.HeaderText = "ShipToCity";
            this.shipToCityDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToCityDataGridViewTextBoxColumn.Name = "shipToCityDataGridViewTextBoxColumn";
            this.shipToCityDataGridViewTextBoxColumn.Visible = false;
            this.shipToCityDataGridViewTextBoxColumn.Width = 125;
            // 
            // shipToCountryRegionCodeDataGridViewTextBoxColumn
            // 
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn.DataPropertyName = "ShipToCountryRegionCode";
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn.HeaderText = "ShipToCountryRegionCode";
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn.Name = "shipToCountryRegionCodeDataGridViewTextBoxColumn";
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn.Visible = false;
            this.shipToCountryRegionCodeDataGridViewTextBoxColumn.Width = 125;
            // 
            // shipmentMethodCodeDataGridViewTextBoxColumn
            // 
            this.shipmentMethodCodeDataGridViewTextBoxColumn.DataPropertyName = "ShipmentMethodCode";
            this.shipmentMethodCodeDataGridViewTextBoxColumn.HeaderText = "ShipmentMethodCode";
            this.shipmentMethodCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipmentMethodCodeDataGridViewTextBoxColumn.Name = "shipmentMethodCodeDataGridViewTextBoxColumn";
            this.shipmentMethodCodeDataGridViewTextBoxColumn.Visible = false;
            this.shipmentMethodCodeDataGridViewTextBoxColumn.Width = 125;
            // 
            // shippingAgentCodeDataGridViewTextBoxColumn
            // 
            this.shippingAgentCodeDataGridViewTextBoxColumn.DataPropertyName = "ShippingAgentCode";
            this.shippingAgentCodeDataGridViewTextBoxColumn.HeaderText = "Shipp.Agent";
            this.shippingAgentCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shippingAgentCodeDataGridViewTextBoxColumn.Name = "shippingAgentCodeDataGridViewTextBoxColumn";
            this.shippingAgentCodeDataGridViewTextBoxColumn.Width = 75;
            // 
            // shippingAgentServiceCodeDataGridViewTextBoxColumn
            // 
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn.DataPropertyName = "ShippingAgentServiceCode";
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn.HeaderText = "ShippingAgentServiceCode";
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn.Name = "shippingAgentServiceCodeDataGridViewTextBoxColumn";
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn.Visible = false;
            this.shippingAgentServiceCodeDataGridViewTextBoxColumn.Width = 125;
            // 
            // shippingNoDataGridViewTextBoxColumn
            // 
            this.shippingNoDataGridViewTextBoxColumn.DataPropertyName = "ShippingNo";
            this.shippingNoDataGridViewTextBoxColumn.HeaderText = "ShippingNo";
            this.shippingNoDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shippingNoDataGridViewTextBoxColumn.Name = "shippingNoDataGridViewTextBoxColumn";
            this.shippingNoDataGridViewTextBoxColumn.Visible = false;
            this.shippingNoDataGridViewTextBoxColumn.Width = 125;
            // 
            // shippingNoSeriesDataGridViewTextBoxColumn
            // 
            this.shippingNoSeriesDataGridViewTextBoxColumn.DataPropertyName = "ShippingNoSeries";
            this.shippingNoSeriesDataGridViewTextBoxColumn.HeaderText = "ShippingNoSeries";
            this.shippingNoSeriesDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shippingNoSeriesDataGridViewTextBoxColumn.Name = "shippingNoSeriesDataGridViewTextBoxColumn";
            this.shippingNoSeriesDataGridViewTextBoxColumn.Visible = false;
            this.shippingNoSeriesDataGridViewTextBoxColumn.Width = 125;
            // 
            // packageTrackingNoDataGridViewTextBoxColumn
            // 
            this.packageTrackingNoDataGridViewTextBoxColumn.DataPropertyName = "PackageTrackingNo";
            this.packageTrackingNoDataGridViewTextBoxColumn.HeaderText = "PackageTrackingNo";
            this.packageTrackingNoDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.packageTrackingNoDataGridViewTextBoxColumn.Name = "packageTrackingNoDataGridViewTextBoxColumn";
            this.packageTrackingNoDataGridViewTextBoxColumn.Visible = false;
            this.packageTrackingNoDataGridViewTextBoxColumn.Width = 125;
            // 
            // shipmentDateDataGridViewTextBoxColumn
            // 
            this.shipmentDateDataGridViewTextBoxColumn.DataPropertyName = "ShipmentDate";
            this.shipmentDateDataGridViewTextBoxColumn.HeaderText = "ShipmentDate";
            this.shipmentDateDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shipmentDateDataGridViewTextBoxColumn.Name = "shipmentDateDataGridViewTextBoxColumn";
            this.shipmentDateDataGridViewTextBoxColumn.Visible = false;
            this.shipmentDateDataGridViewTextBoxColumn.Width = 125;
            // 
            // postingDateDataGridViewTextBoxColumn
            // 
            this.postingDateDataGridViewTextBoxColumn.DataPropertyName = "PostingDate";
            this.postingDateDataGridViewTextBoxColumn.HeaderText = "PostingDate";
            this.postingDateDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.postingDateDataGridViewTextBoxColumn.Name = "postingDateDataGridViewTextBoxColumn";
            this.postingDateDataGridViewTextBoxColumn.Visible = false;
            this.postingDateDataGridViewTextBoxColumn.Width = 125;
            // 
            // requestedDeliveryDateDataGridViewTextBoxColumn
            // 
            this.requestedDeliveryDateDataGridViewTextBoxColumn.DataPropertyName = "RequestedDeliveryDate";
            this.requestedDeliveryDateDataGridViewTextBoxColumn.HeaderText = "RequestedDeliveryDate";
            this.requestedDeliveryDateDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.requestedDeliveryDateDataGridViewTextBoxColumn.Name = "requestedDeliveryDateDataGridViewTextBoxColumn";
            this.requestedDeliveryDateDataGridViewTextBoxColumn.Visible = false;
            this.requestedDeliveryDateDataGridViewTextBoxColumn.Width = 125;
            // 
            // shippedDataGridViewTextBoxColumn
            // 
            this.shippedDataGridViewTextBoxColumn.DataPropertyName = "Shipped";
            this.shippedDataGridViewTextBoxColumn.HeaderText = "Shipped";
            this.shippedDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.shippedDataGridViewTextBoxColumn.Name = "shippedDataGridViewTextBoxColumn";
            this.shippedDataGridViewTextBoxColumn.Visible = false;
            this.shippedDataGridViewTextBoxColumn.Width = 50;
            // 
            // SumStatus
            // 
            this.SumStatus.DataPropertyName = "SumStatus";
            this.SumStatus.HeaderText = "Status";
            this.SumStatus.MinimumWidth = 6;
            this.SumStatus.Name = "SumStatus";
            this.SumStatus.ReadOnly = true;
            this.SumStatus.ToolTipText = "Only \'ReadyForShip\' are ready";
            this.SumStatus.Width = 95;
            // 
            // auxiliaryIndex1DataGridViewTextBoxColumn
            // 
            this.auxiliaryIndex1DataGridViewTextBoxColumn.DataPropertyName = "AuxiliaryIndex1";
            this.auxiliaryIndex1DataGridViewTextBoxColumn.HeaderText = "AuxiliaryIndex1";
            this.auxiliaryIndex1DataGridViewTextBoxColumn.MinimumWidth = 6;
            this.auxiliaryIndex1DataGridViewTextBoxColumn.Name = "auxiliaryIndex1DataGridViewTextBoxColumn";
            this.auxiliaryIndex1DataGridViewTextBoxColumn.Visible = false;
            this.auxiliaryIndex1DataGridViewTextBoxColumn.Width = 125;
            // 
            // OrdersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 500);
            this.Controls.Add(this.signUpButton);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "OrdersForm";
            this.Text = "Order Shipping";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shippingPostalAddressBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource shippingPostalAddressBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToContactDataGridViewTextBoxColumn;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button signUpButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn shippedNotInvoicedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn numberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToName2DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToAddressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToAddress2DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToPostCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToCityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipToCountryRegionCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipmentMethodCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shippingAgentCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shippingAgentServiceCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shippingNoDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shippingNoSeriesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn packageTrackingNoDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shipmentDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn postingDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn requestedDeliveryDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn shippedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SumStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn auxiliaryIndex1DataGridViewTextBoxColumn;
    }
}