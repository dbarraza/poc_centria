import React from 'react';

const ApplicationDetail = ({ detailResponse, showMain }: any) => {
    return (
        <div className="component-container">
            <div className="divTittle">
                <h1>Detalles de la acción</h1>
            </div>
            <div className="field">
                <span className="field-label">ID:</span>
                <span className="field-value">{detailResponse.data.id}</span>
            </div>
            <div className="field">
                <span className="field-label">Nombre:</span>
                <span className="field-value">{detailResponse.data.name}</span>
            </div>
            <div className="field">
                <span className="field-label">Estado:</span>
                <span className="field-value">{detailResponse.data.status}</span>
            </div>
            <div className="field">
                <span className="field-label">Fecha de creación:</span>
                <span className="field-value">{detailResponse.data.createdAt}</span>
            </div>
            <div className="field">
                <span className="field-label">URL:</span>
                <a className="field-link" href={detailResponse.data.excelUrl}>{detailResponse.data.excelUrl}</a>
            </div>
            <div>
                <button onClick={showMain}>Procesos de selección</button>
            </div>
        </div>
    );
}

export default ApplicationDetail;
