function Rating() {
    const request = fetch('https://localhost:7141/rating/data');
    return (
        <div>{request}</div>
    )
}