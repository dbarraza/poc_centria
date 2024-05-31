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
                <span className="field-label">URL Base Excel:</span>
                <a className="field-link" href={detailResponse.data.excelUrl} target='_blank'>ver</a>
            </div>
            <div className='field'>
                <span className="field-label">Descripción del empleo</span>
                <span className="field-value">{detailResponse.data.jobDescription}</span>
            </div>
            <div>
                <button onClick={showMain}>Volver</button>
            </div>
        </div>
    );
}

export default ApplicationDetail;
