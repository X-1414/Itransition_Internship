const positionId = window.__positionId;
const rulesList = document.getElementById('accessRulesList');
const template = document.getElementById('ruleRowTemplate');
let attributeOptions = [];

fetch('/Attributes/ListJson').then(r=>r.json()).then(data=>{
    attributeOptions = data.all;
    loadExistingRules();
});

function addRuleRow(attributeDefinitionId, operatorValue, comparisonValue){
    const clone = template.content.cloneNode(true);
    const select = clone.querySelector('.rule-attribute');
    attributeOptions.forEach(a=>{
        const opt = document.createElement('option');
        opt.value = a.id;
        opt.textContent = `${a.name} (${a.dataType})`;
        select.appendChild(opt);
    });
    if (attributeDefinitionId) select.value = attributeDefinitionId;
    if (operatorValue !== undefined) clone.querySelector('.rule-operator').value = operatorValue;
    if (comparisonValue) clone.querySelector('.rule-value').value = comparisonValue;
    clone.querySelector('.btn-remove-rule').addEventListener('click', (e)=> { e.target.closest('.rule-row').remove();});
    rulesList.appendChild(clone);
}

async function loadExistingRules(){
    const response = await fetch(`/Positions/AccessRulesJson?positionId=${positionId}`);
    const rules = await response.json();
    rules.forEach(r => addRuleRow(r.attributeDefinitionId, r.operatorValue, r.comparisonValue));
}
document.getElementById('btnAddRule').addEventListener('click', ()=>addRuleRow());
document.getElementById('btnSaveRules').addEventListener('click', async()=> {
    const rows = document.querySelectorAll('.rule-row');
    const rules = Array.from(rows).map(row=>({
        attributeDefinitionId: parseInt(row.querySelector('.rule-attribute').value),
        operatorValue: parseInt(row.querySelector('.rule-operator').value),
        comparisonValue: row.querySelector('.rule-value').value
    }));
    const errorBox = document.getElementById('rulesError');
    try{
        const response = await fetch('/Positions/SaveAccessRules', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({positionId, rules})
        });
        if (!response.ok){
            errorBox.textContent = 'Could not save access rules.';
            errorBox.classList.remove('d-none');
            return;
        }
        location.reload();
    } catch(err){
        errorBox.textContent = 'Network error: ' + err.message;
        errorBox.classList.remove('d-none');
    }
});