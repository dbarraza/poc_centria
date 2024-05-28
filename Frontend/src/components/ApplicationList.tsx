import React from 'react';

const ApplicationList = ({ dataTable, showApplicationDetail, showFilterCandidates, showNewProcess }: any) => {
    return (
        <div className="component-container">
            <div className="divTittle">
                <h1>Procesos de selección</h1>
            </div>
            <table className="lista">
                <thead>
                    <tr>
                        <th>Id</th>
                        <th>Nombre</th>
                        <th>Fecha Creación</th>
                        <th>Estado</th>
                        <th>Archivo</th>
                        <th>-</th>
                        <th>-</th>
                    </tr>
                </thead>
                <tbody>
                    {dataTable.map((data: any, index: number) => (
                        <tr key={index}>
                            <td>{data.id}</td>
                            <td>{data.name}</td>
                            <td>{data.createdAt}</td>
                            <td>{data.status}</td>
                            <td><a href={data.excelUrl} target="_blank" rel="noopener noreferrer">Ver archivo</a></td>
                            <td><button className="detalle-btn" onClick={() => showApplicationDetail(data.id)}>Ver detalle</button></td>
                            <td><button className="detalle-btn" onClick={() => showFilterCandidates(data.id)}>Filtrar Candidatos</button></td>
                        </tr>
                    ))}
                </tbody>
            </table>
            <div className="btn-with-margin">
                <button onClick={showNewProcess}>Nuevo Proceso</button>
            </div>
        </div>
    );
}

export default ApplicationList;
