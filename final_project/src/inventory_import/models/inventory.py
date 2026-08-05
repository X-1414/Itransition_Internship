import requests
from odoo import models, fields
from odoo.exceptions import UserError

class Inventory(models.Model):
    _name = 'inventory.import.inventory'
    _description = 'Imported Inventory'

    title = fields.Char(string='Title', required=True)
    source_token = fields.Char(string='Source API Token')
    source_url = fields.Char(string='Source API Base URL', default='http://host.docker.internal:5249')
    field_ids = fields.One2many('inventory.import.field', 'inventory_id', string='Fields')

    def action_import_from_api(self):
        self.ensure_one()
        if not self.source_token:
            raise UserError('Please provide an API token before importing.')
        url = f"{self.source_url}/api/inventories/{self.source_token}"
        try:
            response = requests.get(url, timeout=10)
        except requests.RequestException as e:
            raise UserError(f'Could not reach the API: {e}')
        if response.status_code != 200:
            raise UserError(f'API returned an error: {response.status_code} - {response.text}')
        data = response.json()
        self.title = data.get('inventoryTitle', self.title)
        self.field_ids.unlink()
        fields_data = data.get('fields', [])
        aggregates_data = data.get('aggregatedResults', [])
        aggregates_by_name = {a['fieldName']: a for a in aggregates_data}
        for f in fields_data:
            agg = aggregates_by_name.get(f['title'], {})
            self.env['inventory.import.field'].create({
                'inventory_id': self.id,
                'field_title': f['title'],
                'field_type': f['type'],
                'average_value': agg.get('average'),
                'min_value': agg.get('min'),
                'max_value': agg.get('max'),
                'top_values': ', '.join(agg.get('topValues') or []) if agg.get('topValues') else None,
            })

class InventoryField(models.Model):
    _name = 'inventory.import.field'
    _description = 'Imported Inventory Field'

    inventory_id = fields.Many2one('inventory.import.inventory', string='Inventory', ondelete = 'cascade')
    field_title = fields.Char(string='Field Title')
    field_type = fields.Char(string='Field Type')
    average_value = fields.Char(string='Average')
    min_value = fields.Char(string='Min')
    max_value = fields.Char(string='Max')
    top_values = fields.Char(string='Top Values')