const ApplicationList = ({ dataTable, showApplicationDetail, showFilterCandidates, showCvProcessing, showNewProcess }: any) => {
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
                        <th>-</th>
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
                            <td><button className="detalle-btn" onClick={() => showApplicationDetail(data.id)}>Ver detalle</button></td>
                            <td><button className="detalle-btn" onClick={() => showFilterCandidates(data.id)}>Filtrar Candidatos</button></td>
                            <td><button className="detalle-btn" onClick={() => showCvProcessing(data.id)}>Cargar CVs</button></td>
                        </tr>
                    ))}
                </tbody>
            </table>
            <div className="btn-with-margin">
                <button onClick={showNewProcess}>Volver</button>
            </div>
        </div>
    );
}

export default ApplicationList;
