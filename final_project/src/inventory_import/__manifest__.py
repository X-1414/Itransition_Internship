{
    'name': 'Inventory Import',
    'version': '1.0',
    'summary': 'Read-only viewer for inventories imported from an external CV Management System API',
    'depends': ['base'],
    'data': ['security/ir.model.access.csv', 'views/inventory_views.xml',],
    'installable': True,
    'application': True,
}